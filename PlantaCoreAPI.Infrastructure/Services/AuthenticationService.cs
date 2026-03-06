using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Auth;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Utils;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Services.External;

namespace PlantaCoreAPI.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioTokenRefresh _repositorioTokenRefresh;
    private readonly IJwtService _servicioJwt;
    private readonly IEmailService _servicioEmail;
    private readonly IPasswordHashService _passwordHashService;

    public AuthenticationService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioTokenRefresh repositorioTokenRefresh,
        IJwtService servicioJwt,
        IEmailService servicioEmail,
        IPasswordHashService passwordHashService)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioTokenRefresh = repositorioTokenRefresh;
        _servicioJwt = servicioJwt;
        _servicioEmail = servicioEmail;
        _passwordHashService = passwordHashService;
    }

    public async Task<Resultado<LoginDTOSaida>> RegistrarAsync(RegistroDTOEntrada entrada)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entrada.Nome) || string.IsNullOrWhiteSpace(entrada.Email) || string.IsNullOrWhiteSpace(entrada.Senha))
                return Resultado<LoginDTOSaida>.Erro("Nome, email e senha são obrigatórios");

            if (entrada.Senha != entrada.ConfirmacaoSenha)
                return Resultado<LoginDTOSaida>.Erro("Senhas não coincidem");

            if (!entrada.Email.Contains("@") || !entrada.Email.Contains("."))
                return Resultado<LoginDTOSaida>.Erro("Formato de email inválido");

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

            await _repositorioUsuario.AdicionarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            usuario.GerarTokenConfirmacaoEmail();
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            var tokenConfirmacao = usuario.TokenConfirmacaoEmail;
            var urlConfirmacao = $"http://localhost:3000/confirmar-email?usuarioId={usuario.Id}&token={tokenConfirmacao}";
            var corpoEmail = EmailTemplateGenerator.GerarEmailConfirmacao(usuario.Nome, urlConfirmacao, tokenConfirmacao);
            
            try
            {
                await _servicioEmail.EnviarAsync(emailNormalizado, "Bem-vindo ao PlantaCore - Confirme sua conta", corpoEmail);
            }
            catch { }

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
            return Resultado<LoginDTOSaida>.Erro($"Erro ao registrar: {ex.Message}");
        }
    }

    public async Task<Resultado<LoginDTOSaida>> LoginAsync(LoginDTOEntrada entrada)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entrada.Email) || string.IsNullOrWhiteSpace(entrada.Senha))
                return Resultado<LoginDTOSaida>.Erro("Email e senha são obrigatórios");

            var emailNormalizado = entrada.Email.ToLower().Trim();
            var usuario = await _repositorioUsuario.ObterPorEmailAsync(emailNormalizado);

            if (usuario == null)
                return Resultado<LoginDTOSaida>.Erro("Email ou senha inválidos");

            if (!usuario.VerificarSenha(entrada.Senha, _passwordHashService.Verify))
                return Resultado<LoginDTOSaida>.Erro("Email ou senha inválidos");

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
            return Resultado<LoginDTOSaida>.Erro($"Erro ao fazer login: {ex.Message}");
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
            return Resultado<LoginDTOSaida>.Erro($"Erro ao renovar token: {ex.Message}");
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
            return Resultado.Erro($"Erro ao fazer logout: {ex.Message}");
        }
    }

    public async Task<Resultado> ConfirmarEmailAsync(ConfirmarEmailDTOEntrada entrada)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(entrada.UsuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            if (usuario.TokenConfirmacaoEmail != entrada.Token)
                return Resultado.Erro("Token inválido");

            usuario.ConfirmarEmail();
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            return Resultado.Ok("Email confirmado com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao confirmar email: {ex.Message}");
        }
    }

    public async Task<Resultado> ResetarSenhaAsync(ResetarSenhaDTOEntrada entrada)
    {
        try
        {
            var emailNormalizado = entrada.Email.ToLower().Trim();
            var usuario = await _repositorioUsuario.ObterPorEmailAsync(emailNormalizado);

            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            usuario.GerarTokenResetarSenha();
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            var tokenReset = usuario.TokenResetarSenha;
            var urlReset = $"http://localhost:3000/resetar-senha?usuarioId={usuario.Id}&token={tokenReset}";
            var corpoEmail = EmailTemplateGenerator.GerarEmailResetarSenha(usuario.Nome, urlReset, tokenReset);
            
            try
            {
                await _servicioEmail.EnviarAsync(emailNormalizado, "PlantaCore - Recuperar Senha", corpoEmail);
            }
            catch
            {

            }

            return Resultado.Ok("Email de reset enviado");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao resetar senha: {ex.Message}");
        }
    }

    public async Task<Resultado> NovaSenhaAsync(NovaSenhaDTOEntrada entrada)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(entrada.UsuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

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
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao definir nova senha: {ex.Message}");
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
            return Resultado.Erro($"Erro ao trocar senha: {ex.Message}");
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

            var urlConfirmacao = $"http://localhost:3000/confirmar-email?usuarioId={usuario.Id}&token={usuario.TokenConfirmacaoEmail}";
            var corpoEmail = EmailTemplateGenerator.GerarEmailConfirmacao(usuario.Nome, urlConfirmacao, usuario.TokenConfirmacaoEmail);

            try
            {
                await _servicioEmail.EnviarAsync(emailNormalizado, "PlantaCore - Confirme seu email", corpoEmail);
            }
            catch { }

            return Resultado.Ok("Se o email existir, um novo link será enviado");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao reenviar confirmação: {ex.Message}");
        }
    }
}
