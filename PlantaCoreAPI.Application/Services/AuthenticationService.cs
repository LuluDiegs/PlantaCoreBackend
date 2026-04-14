using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Auth;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Utils;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PlantaCoreAPI.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioTokenRefresh _repositorioTokenRefresh;
    private readonly IJwtService _servicioJwt;
    private readonly IEmailService _servicioEmail;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly string _urlFrontend;
    private readonly string _googleClientId;

    public AuthenticationService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioTokenRefresh repositorioTokenRefresh,
        IJwtService servicioJwt,
        IEmailService servicioEmail,
        IPasswordHashService passwordHashService,
        ILogger<AuthenticationService> logger,
        IConfiguration configuration)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioTokenRefresh = repositorioTokenRefresh;
        _servicioJwt = servicioJwt;
        _servicioEmail = servicioEmail;
        _passwordHashService = passwordHashService;
        _logger = logger;
        _urlFrontend = configuration["Frontend:Url"] ?? "http://localhost:5173";
        _googleClientId = configuration["Google:ClientId"] ?? string.Empty;
    }

    public async Task<Resultado<LoginDTOSaida>> RegistrarAsync(RegistroDTOEntrada entrada)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entrada.Nome) || string.IsNullOrWhiteSpace(entrada.Email) || string.IsNullOrWhiteSpace(entrada.Senha))
                return Resultado<LoginDTOSaida>.Erro("Nome, email e senha são obrigatórios");
            if (entrada.Senha != entrada.ConfirmacaoSenha)
                return Resultado<LoginDTOSaida>.Erro("Senhas não coincidem");
            var emailNormalizado = entrada.Email.ToLower().Trim();
            if (await _repositorioUsuario.EmailJaExisteAsync(emailNormalizado))
                return Resultado<LoginDTOSaida>.Erro("Email já registrado");
            if (!PasswordValidator.ValidarComplexidade(entrada.Senha))
            {
                var mensagem = PasswordValidator.ObterMensagemErro(entrada.Senha);
                return Resultado<LoginDTOSaida>.Erro(mensagem);
            }

            var senhaHash = _passwordHashService.Hash(entrada.Senha);
            var usuario = Usuario.Criar(entrada.Nome, emailNormalizado, senhaHash);
            usuario.GerarTokenConfirmacaoEmail();
            await _repositorioUsuario.AdicionarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();
            var tokenConfirmacao = usuario.TokenConfirmacaoEmail ?? string.Empty;
            var urlConfirmacao = $"{_urlFrontend}/confirmar-email?usuarioId={usuario.Id}&token={tokenConfirmacao}";
            var corpoEmail = EmailTemplateGenerator.GerarEmailConfirmacao(usuario.Nome, urlConfirmacao, tokenConfirmacao);
            try
            {
                await _servicioEmail.EnviarAsync(emailNormalizado, "Bem-vindo ao PlantaCore - Confirme sua conta", corpoEmail);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Falha ao enviar email de confirmação para {Email}", EmailMascarador.Mascarar(emailNormalizado));
            }

            return Resultado<LoginDTOSaida>.Ok(new LoginDTOSaida
            {
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                TokenAcesso = string.Empty,
                TokenRefresh = string.Empty
            }, "Usuário registrado com sucesso. Verifique seu email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário");
            return Resultado<LoginDTOSaida>.Erro("Erro ao registrar. Tente novamente.");
        }
    }

    public async Task<Resultado<LoginDTOSaida>> LoginAsync(LoginDTOEntrada entrada)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entrada.Email) || string.IsNullOrWhiteSpace(entrada.Senha))
                return Resultado<LoginDTOSaida>.Erro("Email e senha são obrigatórios");
            var emailNormalizado = entrada.Email.ToLower().Trim();
            var usuario = await _repositorioUsuario.ObterPorEmailIncluindoInativosAsync(emailNormalizado);
            if (usuario == null)
                return Resultado<LoginDTOSaida>.Erro("Conta não encontrada. Verifique o email ou crie uma nova conta.");
            if (!usuario.VerificarSenha(entrada.Senha, _passwordHashService.Verify))
                return Resultado<LoginDTOSaida>.Erro("Email ou senha inválidos");
            if (!usuario.Ativo)
                return Resultado<LoginDTOSaida>.Erro("Sua conta está desativada. Use a opção 'Reativar conta' para recuperar o acesso.");
            if (!usuario.EmailConfirmado)
                return Resultado<LoginDTOSaida>.Erro("Email não confirmado");
            var tokenAcesso = _servicioJwt.GerarTokenAcesso(usuario.Id, usuario.Email, usuario.Nome);
            var tokenRefresh = _servicioJwt.GerarTokenRefresh();
            var tokenRefreshEntidade = TokenRefresh.Criar(usuario.Id, tokenRefresh);
            await _repositorioTokenRefresh.AdicionarAsync(tokenRefreshEntidade);
            await _repositorioTokenRefresh.SalvarMudancasAsync();
            return Resultado<LoginDTOSaida>.Ok(new LoginDTOSaida
            {
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                TokenAcesso = tokenAcesso,
                TokenRefresh = tokenRefresh
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login");
            return Resultado<LoginDTOSaida>.Erro("Erro ao realizar login. Tente novamente.");
        }
    }

    public async Task<Resultado<LoginDTOSaida>> LoginComGoogleAsync(string tokenDoGoogle)
        {
            try
            {
                var configuracoes = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _googleClientId } 
                };

                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(tokenDoGoogle, configuracoes);

                var emailNormalizado = payload.Email.ToLower().Trim();
                var usuario = await _repositorioUsuario.ObterPorEmailIncluindoInativosAsync(emailNormalizado);

                if (usuario == null)
                {
                    usuario = Usuario.CriarComGoogle(payload.Name, emailNormalizado, payload.Picture);
                    await _repositorioUsuario.AdicionarAsync(usuario);
                    await _repositorioUsuario.SalvarMudancasAsync();
                }
                else if (!usuario.Ativo)
                {
                    return Resultado<LoginDTOSaida>.Erro("Sua conta está desativada.");
                }

                var tokenAcesso = _servicioJwt.GerarTokenAcesso(usuario.Id, usuario.Email, usuario.Nome);
                var tokenRefresh = _servicioJwt.GerarTokenRefresh();
                
                var tokenRefreshEntidade = TokenRefresh.Criar(usuario.Id, tokenRefresh);
                await _repositorioTokenRefresh.AdicionarAsync(tokenRefreshEntidade);
                await _repositorioTokenRefresh.SalvarMudancasAsync();

                return Resultado<LoginDTOSaida>.Ok(new LoginDTOSaida
                {
                    UsuarioId = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    TokenAcesso = tokenAcesso,
                    TokenRefresh = tokenRefresh
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar login com o Google");
                return Resultado<LoginDTOSaida>.Erro("Token do Google inválido ou expirado.");
            }
        }

    public async Task<Resultado<LoginDTOSaida>> RefreshTokenAsync(string tokenRefresh)
    {
        try
        {
            var tokenRefreshEntidade = await _repositorioTokenRefresh.ObterPorTokenAsync(tokenRefresh);
            if (tokenRefreshEntidade == null || !tokenRefreshEntidade.EstaValido())
                return Resultado<LoginDTOSaida>.Erro("Token de refresh inválido ou expirado");
            var usuario = await _repositorioUsuario.ObterPorIdAsync(tokenRefreshEntidade.UsuarioId);
            if (usuario == null)
                return Resultado<LoginDTOSaida>.Erro("Usuário não encontrado");
            var novoTokenAcesso = _servicioJwt.GerarTokenAcesso(usuario.Id, usuario.Email, usuario.Nome);
            var novoTokenRefresh = _servicioJwt.GerarTokenRefresh();
            tokenRefreshEntidade.Revogar();
            await _repositorioTokenRefresh.AtualizarAsync(tokenRefreshEntidade);
            var novaTokenRefreshEntidade = TokenRefresh.Criar(usuario.Id, novoTokenRefresh);
            await _repositorioTokenRefresh.AdicionarAsync(novaTokenRefreshEntidade);
            await _repositorioTokenRefresh.SalvarMudancasAsync();
            return Resultado<LoginDTOSaida>.Ok(new LoginDTOSaida
            {
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                TokenAcesso = novoTokenAcesso,
                TokenRefresh = novoTokenRefresh
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return Resultado<LoginDTOSaida>.Erro("Erro ao renovar token. Tente novamente.");
        }
    }

    public async Task<Resultado> LogoutAsync(Guid usuarioId)
    {
        try
        {
            await _repositorioTokenRefresh.RevogarTokensUsuarioAsync(usuarioId);
            return Resultado.Ok("Logout realizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar logout");
            return Resultado.Erro("Erro ao realizar logout. Tente novamente.");
        }
    }

    public async Task<Resultado> ConfirmarEmailAsync(ConfirmarEmailDTOEntrada entrada)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(entrada.UsuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");
            if (!TokensIguais(usuario.TokenConfirmacaoEmail, entrada.Token))
                return Resultado.Erro("Token inválido");
            usuario.ConfirmarEmail();
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();
            return Resultado.Ok("Email confirmado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar email");
            return Resultado.Erro("Erro ao confirmar email. Tente novamente.");
        }
    }

    public async Task<Resultado> ResetarSenhaAsync(ResetarSenhaDTOEntrada entrada)
    {
        try
        {
            var emailNormalizado = entrada.Email.ToLower().Trim();
            var usuario = await _repositorioUsuario.ObterPorEmailAsync(emailNormalizado);
            if (usuario == null)
                return Resultado.Ok("Se o email existir, um link de recuperação será enviado");
            usuario.GerarTokenResetarSenha();
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();
            var tokenReset = usuario.TokenResetarSenha ?? string.Empty;
            var urlReset = $"{_urlFrontend}/resetar-senha?usuarioId={usuario.Id}&token={tokenReset}";
            var corpoEmail = EmailTemplateGenerator.GerarEmailResetarSenha(usuario.Nome, urlReset, tokenReset);
            try
            {
                await _servicioEmail.EnviarAsync(emailNormalizado, "PlantaCore - Recuperar Senha", corpoEmail);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Falha ao enviar email de reset para {Email}", EmailMascarador.Mascarar(emailNormalizado));
            }

            return Resultado.Ok("Se o email existir, um link de recuperação será enviado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao solicitar reset de senha");
            return Resultado.Erro("Erro ao solicitar reset de senha. Tente novamente.");
        }
    }

    public async Task<Resultado> NovaSenhaAsync(NovaSenhaDTOEntrada entrada)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(entrada.UsuarioId);
            if (usuario == null)
                return Resultado.Erro("Token inválido ou expirado");
            if (string.IsNullOrWhiteSpace(usuario.TokenResetarSenha))
                return Resultado.Erro("Token inválido ou expirado");
            if (!TokensIguais(usuario.TokenResetarSenha, entrada.Token))
                return Resultado.Erro("Token inválido ou expirado");
            if (entrada.NovaSenha != entrada.ConfirmacaoSenha)
                return Resultado.Erro("Senhas não coincidem");
            if (!PasswordValidator.ValidarComplexidade(entrada.NovaSenha))
            {
                var mensagem = PasswordValidator.ObterMensagemErro(entrada.NovaSenha);
                return Resultado.Erro(mensagem);
            }

            var senhaHash = _passwordHashService.Hash(entrada.NovaSenha);
            usuario.ResetarSenha(senhaHash);
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();
            return Resultado.Ok("Senha alterada com sucesso");
        }
        catch (PlantaCoreAPI.Domain.Exceptions.DomainException ex)
        {
            return Resultado.Erro(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir nova senha");
            return Resultado.Erro("Erro ao definir nova senha. Tente novamente.");
        }
    }

    public async Task<Resultado> TrocarSenhaAsync(Guid usuarioId, TrocarSenhaDTOEntrada entrada)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");
            if (!usuario.VerificarSenha(entrada.SenhaAtual, _passwordHashService.Verify))
                return Resultado.Erro("Senha atual inválida");
            if (entrada.NovaSenha != entrada.ConfirmacaoSenha)
                return Resultado.Erro("Novas senhas não coincidem");
            if (!PasswordValidator.ValidarComplexidade(entrada.NovaSenha))
            {
                var mensagem = PasswordValidator.ObterMensagemErro(entrada.NovaSenha);
                return Resultado.Erro(mensagem);
            }

            if (usuario.VerificarSenha(entrada.NovaSenha, _passwordHashService.Verify))
                return Resultado.Erro("Nova senha não pode ser igual à senha atual");
            var novaSenhaHash = _passwordHashService.Hash(entrada.NovaSenha);
            usuario.TrocarSenha(novaSenhaHash);
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();
            return Resultado.Ok("Senha alterada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao trocar senha");
            return Resultado.Erro("Erro ao trocar senha. Tente novamente.");
        }
    }

    public async Task<Resultado> ReenviarConfirmacaoEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return Resultado.Erro("Email é obrigatório");
            var emailNormalizado = email.ToLower().Trim();
            var usuario = await _repositorioUsuario.ObterPorEmailAsync(emailNormalizado);
            if (usuario == null)
                return Resultado.Ok("Se o email existir, um novo link será enviado");
            if (usuario.EmailConfirmado)
                return Resultado.Erro("Este email já foi confirmado");
            usuario.GerarTokenConfirmacaoEmail();
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();
            var urlConfirmacao = $"{_urlFrontend}/confirmar-email?usuarioId={usuario.Id}&token={usuario.TokenConfirmacaoEmail}";
            var corpoEmail = EmailTemplateGenerator.GerarEmailConfirmacao(usuario.Nome, urlConfirmacao, usuario.TokenConfirmacaoEmail ?? string.Empty);
            try
            {
                await _servicioEmail.EnviarAsync(emailNormalizado, "PlantaCore - Confirme seu email", corpoEmail);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Falha ao reenviar email de confirmação para {Email}", EmailMascarador.Mascarar(emailNormalizado));
            }

            return Resultado.Ok("Se o email existir, um novo link será enviado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reenviar confirmação de email");
            return Resultado.Erro("Erro ao reenviar confirmação. Tente novamente.");
        }
    }

    private static bool TokensIguais(string? a, string? b)
    {
        if (a == null || b == null)
            return false;
        var bytesA = Encoding.UTF8.GetBytes(a);
        var bytesB = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}
