using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Utils;
using PlantaCoreAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Infrastructure.Dados;
using Microsoft.Extensions.Configuration;

namespace PlantaCoreAPI.Infrastructure.Services;

public class AccountReactivationService : IAccountReactivationService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IEmailService _emailService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly PlantaCoreDbContext _contexto;
    private readonly string _urlFrontend;

    public AccountReactivationService(
        IRepositorioUsuario repositorioUsuario,
        IEmailService emailService,
        IPasswordHashService passwordHashService,
        PlantaCoreDbContext contexto,
        IConfiguration configuration)
    {
        _repositorioUsuario = repositorioUsuario;
        _emailService = emailService;
        _passwordHashService = passwordHashService;
        _contexto = contexto;
        _urlFrontend = configuration["Frontend:Url"] ?? "http://localhost:5173";
    }

    public async Task<Resultado> SolicitarReativacaoAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return Resultado.Erro("Email não pode estar vazio");

            email = email.ToLower().Trim();

            var usuario = await _repositorioUsuario.ObterPorEmailAsync(email);

            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            if (usuario.Ativo)
                return Resultado.Erro("Sua conta já está ativa");

            usuario.GerarTokenResetarSenha();

            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            var urlReativacao = $"{_urlFrontend}/reativar-conta?email={Uri.EscapeDataString(email)}&token={usuario.TokenResetarSenha}";
            var corpoEmail = GerarEmailReativacao(usuario.Nome, urlReativacao, usuario.TokenResetarSenha);

            var emailEnviado = await _emailService.EnviarAsync(
                email,
                "PlantaCore - Reative sua conta",
                corpoEmail);

            if (!emailEnviado)
                return Resultado.Erro("Erro ao enviar email de reativação. Tente novamente mais tarde.");

            return Resultado.Ok("Email de reativação enviado com sucesso. Verifique sua caixa de entrada.");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao solicitar reativação: {ex.Message}");
        }
    }

    public async Task<Resultado> ReativarComTokenAsync(string email, string token, string novaSenha)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return Resultado.Erro("Email não pode estar vazio");

            if (string.IsNullOrWhiteSpace(token))
                return Resultado.Erro("Token não pode estar vazio");

            if (string.IsNullOrWhiteSpace(novaSenha))
                return Resultado.Erro("Senha não pode estar vazia");

            email = email.ToLower().Trim();

            var usuario = await _repositorioUsuario.ObterPorEmailAsync(email);

            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            if (string.IsNullOrWhiteSpace(usuario.TokenResetarSenha) || usuario.TokenResetarSenha != token)
                return Resultado.Erro("Token de reativação inválido");

            if (usuario.DataTokenResetarSenha < DateTime.UtcNow)
                return Resultado.Erro("Token de reativação expirou. Solicite um novo.");

            if (!PasswordValidator.ValidarComplexidade(novaSenha))
            {
                var mensagem = PasswordValidator.ObterMensagemErro(novaSenha);
                return Resultado.Erro($"Senha fraca: {mensagem}");
            }

            var senhaHash = _passwordHashService.Hash(novaSenha);

            usuario.TrocarSenha(senhaHash);
            usuario.Reativar();

            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            var corpoEmail = GerarEmailReativacaoConfirmada(usuario.Nome);
            await _emailService.EnviarAsync(email, "PlantaCore - Conta Reativada com Sucesso", corpoEmail);

            return Resultado.Ok("Conta reativada com sucesso! Sua senha foi atualizada. Você pode fazer login agora.");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao reativar conta: {ex.Message}");
        }
    }

    public async Task<Resultado> VerificarTokenReativacaoAsync(string email, string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return Resultado.Erro("Email não pode estar vazio");

            if (string.IsNullOrWhiteSpace(token))
                return Resultado.Erro("Token não pode estar vazio");

            email = email.ToLower().Trim();

            var usuario = await _repositorioUsuario.ObterPorEmailAsync(email);

            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            if (string.IsNullOrWhiteSpace(usuario.TokenResetarSenha) || usuario.TokenResetarSenha != token)
                return Resultado.Erro("Token inválido");

            if (usuario.DataTokenResetarSenha < DateTime.UtcNow)
                return Resultado.Erro("Token expirado");

            if (usuario.Ativo)
                return Resultado.Erro("A conta já está ativa");

            return Resultado.Ok("Token válido");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao verificar token: {ex.Message}");
        }
    }

    public async Task<Resultado> ResetarSenhaSemTokenAsync(string email, string novaSenha)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return Resultado.Erro("Email não pode estar vazio");

            if (string.IsNullOrWhiteSpace(novaSenha))
                return Resultado.Erro("Senha não pode estar vazia");

            email = email.ToLower().Trim();

            var usuario = await _repositorioUsuario.ObterPorEmailAsync(email);

            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            if (!PasswordValidator.ValidarComplexidade(novaSenha))
            {
                var mensagem = PasswordValidator.ObterMensagemErro(novaSenha);
                return Resultado.Erro($"Senha fraca: {mensagem}");
            }

            var senhaHash = _passwordHashService.Hash(novaSenha);

            usuario.TrocarSenha(senhaHash);

            if (!usuario.Ativo)
            {
                usuario.Reativar();
            }

            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            return Resultado.Ok("Senha resetada com sucesso!");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao resetar senha: {ex.Message}");
        }
    }

    private static string GerarEmailReativacao(string nome, string urlReativacao, string token)
    {
        return $@"
            <!DOCTYPE html>
            <html lang=""pt-BR"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 20px auto; background-color: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #27ae60 0%, #229954 100%); color: white; padding: 50px 20px; text-align: center; }}
                    .header h1 {{ margin: 0; font-size: 32px; font-weight: bold; }}
                    .header p {{ margin: 8px 0 0 0; font-size: 14px; opacity: 0.95; }}
                    .content {{ padding: 40px 20px; }}
                    .greeting {{ color: #27ae60; font-size: 18px; font-weight: bold; margin-bottom: 15px; }}
                    .content p {{ color: #333; font-size: 16px; line-height: 1.8; margin: 15px 0; }}
                    .highlight {{ color: #27ae60; font-weight: bold; }}
                    .button-container {{ text-align: center; margin: 35px 0; }}
                    .button {{ background: linear-gradient(135deg, #27ae60 0%, #229954 100%); color: white; padding: 16px 45px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold; display: inline-block; box-shadow: 0 4px 10px rgba(39, 174, 96, 0.3); }}
                    .info-box {{ background-color: #e8f8f5; border-left: 4px solid #27ae60; border-radius: 5px; padding: 15px 20px; margin: 20px 0; }}
                    .info-box p {{ margin: 8px 0; color: #1e5631; font-size: 14px; }}
                    .divider {{ border-bottom: 2px solid #e0e0e0; margin: 30px 0; }}
                    .footer {{ background-color: #f8f9fa; padding: 25px 20px; text-align: center; color: #666; font-size: 12px; border-top: 1px solid #e0e0e0; }}
                    .footer-text {{ margin: 5px 0; }}
                    .warning {{ color: #d32f2f; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>PlantaCore</h1>
                        <p>Reativação de Conta</p>
                    </div>
                    <div class=""content"">
                        <p class=""greeting"">Olá <span class=""highlight"">{nome}</span>,</p>
                        <p>Recebemos uma solicitação para reativar sua conta no <span class=""highlight"">PlantaCore</span>.</p>
                        <p>Para reativar sua conta e definir uma nova senha, clique no botão abaixo:</p>
                        <div class=""button-container"">
                            <a href=""{urlReativacao}"" class=""button"">Reativar Minha Conta</a>
                        </div>
                        <div class=""info-box"">
                            <p><strong>O que você pode fazer após reativar:</strong></p>
                            <p>? Acessar sua conta com a nova senha</p>
                            <p>? Visualizar todas as suas plantas e posts</p>
                            <p>? Continuar sua jornada no PlantaCore</p>
                        </div>
                        <p style=""color: #999; font-size: 13px; text-align: center;"">Este link expira em 1 hora por razões de segurança.</p>
                        <div class=""divider""></div>
                        <p class=""warning"">Se você não solicitou reativar sua conta, por favor ignore este e-mail.</p>
                        <p style=""font-size: 14px; color: #666;"">Dúvidas? <a href=""mailto:squadhackathonio@gmail.com"" style=""color: #27ae60; text-decoration: none;"">Entre em contato conosco</a></p>
                    </div>
                    <div class=""footer"">
                        <p class=""footer-text""><strong>PlantaCore</strong> © 2026 - Seu app de plantas inteligente</p>
                        <p class=""footer-text"">Segurança e privacidade em primeiro lugar</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private static string GerarEmailReativacaoConfirmada(string nome)
    {
        return $@"
            <!DOCTYPE html>
            <html lang=""pt-BR"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 20px auto; background-color: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #27ae60 0%, #229954 100%); color: white; padding: 50px 20px; text-align: center; }}
                    .header h1 {{ margin: 0; font-size: 32px; font-weight: bold; }}
                    .header p {{ margin: 8px 0 0 0; font-size: 14px; opacity: 0.95; }}
                    .content {{ padding: 40px 20px; }}
                    .greeting {{ color: #27ae60; font-size: 18px; font-weight: bold; margin-bottom: 15px; }}
                    .content p {{ color: #333; font-size: 16px; line-height: 1.8; margin: 15px 0; }}
                    .highlight {{ color: #27ae60; font-weight: bold; }}
                    .success-box {{ background-color: #e8f8f5; border-left: 4px solid #27ae60; border-radius: 5px; padding: 15px 20px; margin: 20px 0; }}
                    .success-box p {{ margin: 8px 0; color: #1e5631; font-size: 14px; }}
                    .divider {{ border-bottom: 2px solid #e0e0e0; margin: 30px 0; }}
                    .footer {{ background-color: #f8f9fa; padding: 25px 20px; text-align: center; color: #666; font-size: 12px; border-top: 1px solid #e0e0e0; }}
                    .footer-text {{ margin: 5px 0; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>PlantaCore</h1>
                        <p>Conta Reativada com Sucesso! ?</p>
                    </div>
                    <div class=""content"">
                        <p class=""greeting"">Olá <span class=""highlight"">{nome}</span>,</p>
                        <p>Sua conta foi <span class=""highlight"">reativada com sucesso</span>!</p>
                        <p>Sua senha foi atualizada conforme solicitado. Você pode fazer login agora com sua nova senha.</p>
                        <div class=""success-box"">
                            <p><strong>? Sua conta está totalmente ativa!</strong></p>
                            <p>Todos os seus dados, plantas e posts foram restaurados.</p>
                            <p>Bem-vindo de volta ao PlantaCore!</p>
                        </div>
                        <div class=""divider""></div>
                        <p style=""font-size: 14px; color: #666;"">Dúvidas? <a href=""mailto:squadhackathonio@gmail.com"" style=""color: #27ae60; text-decoration: none;"">Entre em contato conosco</a></p>
                    </div>
                    <div class=""footer"">
                        <p class=""footer-text""><strong>PlantaCore</strong> © 2026 - Seu app de plantas inteligente</p>
                        <p class=""footer-text"">Segurança e privacidade em primeiro lugar</p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
