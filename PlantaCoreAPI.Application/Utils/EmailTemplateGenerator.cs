namespace PlantaCoreAPI.Application.Utils;

public static class EmailTemplateGenerator
{
    public static string GerarEmailConfirmacao(string nome, string urlConfirmacao, string token)
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
                    .header-logo {{ font-size: 48px; margin-bottom: 15px; }}
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
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>PlantaCore</h1>
                        <p>Bem-vindo ao mundo das plantas!</p>
                    </div>
                    <div class=""content"">
                        <p class=""greeting"">Olá <span class=""highlight"">{nome}</span>,</p>
                        <p>Estamos muito felizes em ter você no <span class=""highlight"">PlantaCore</span>! Prepare-se para uma jornada incrível explorando o mundo fascinante das plantas.</p>
                        <p>Para começar, precisamos confirmar sua conta. Clique no botão abaixo para validar seu endereço de e-mail:</p>
                        <div class=""button-container"">
                            <a href=""{urlConfirmacao}"" class=""button"">Confirmar Minha Conta</a>
                        </div>
                        <div class=""info-box"">
                            <p><strong>O que você pode fazer:</strong></p>
                            <p>Identificar plantas por foto usando IA</p>
                            <p>Descobrir informações detalhadas sobre toxicidade</p>
                            <p>Receber dicas de cuidados personalizadas</p>
                            <p>Criar sua coleção de plantas favoritas</p>
                            <p>Conectar com outros entusiastas</p>
                        </div>
                        <p style=""color: #999; font-size: 13px; text-align: center;"">Este link expira em 24 horas por razões de segurança.</p>
                        <div class=""divider""></div>
                        <p style=""font-size: 14px; color: #666;"">Se você não se registrou no PlantaCore, por favor ignore este e-mail.</p>
                        <p style=""font-size: 14px; color: #666;"">Dúvidas? <a href=""mailto:squadhackathonio@gmail.com"" style=""color: #27ae60; text-decoration: none;"">Entre em contato conosco</a></p>
                    </div>
                    <div class=""footer"">
                        <p class=""footer-text""><strong>PlantaCore</strong> &copy; 2026 - Seu app de plantas inteligente</p>
                        <p class=""footer-text"">Segurança e privacidade em primeiro lugar</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    public static string GerarEmailResetarSenha(string nome, string urlReset, string token)
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
                    .header-logo {{ font-size: 48px; margin-bottom: 15px; }}
                    .header h1 {{ margin: 0; font-size: 32px; font-weight: bold; }}
                    .header p {{ margin: 8px 0 0 0; font-size: 14px; opacity: 0.95; }}
                    .content {{ padding: 40px 20px; }}
                    .greeting {{ color: #27ae60; font-size: 18px; font-weight: bold; margin-bottom: 15px; }}
                    .content p {{ color: #333; font-size: 16px; line-height: 1.8; margin: 15px 0; }}
                    .highlight {{ color: #27ae60; font-weight: bold; }}
                    .button-container {{ text-align: center; margin: 35px 0; }}
                    .button {{ background: linear-gradient(135deg, #27ae60 0%, #229954 100%); color: white; padding: 16px 45px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold; display: inline-block; box-shadow: 0 4px 10px rgba(39, 174, 96, 0.3); }}
                    .info-box {{ background-color: #f8d7da; border-left: 4px solid #f5576c; border-radius: 5px; padding: 15px 20px; margin: 20px 0; }}
                    .info-box p {{ margin: 8px 0; color: #721c24; font-size: 14px; }}
                    .divider {{ border-bottom: 2px solid #e0e0e0; margin: 30px 0; }}
                    .footer {{ background-color: #f8f9fa; padding: 25px 20px; text-align: center; color: #666; font-size: 12px; border-top: 1px solid #e0e0e0; }}
                    .footer-text {{ margin: 5px 0; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>PlantaCore</h1>
                        <p>Recuperação de Senha</p>
                    </div>
                    <div class=""content"">
                        <p class=""greeting"">Olá <span class=""highlight"">{nome}</span>,</p>
                        <p>Recebemos uma solicitação para redefinir sua senha. Se não foi você, pode ignorar este email com segurança.</p>
                        <p>Para redefinir sua senha, clique no botão abaixo:</p>
                        <div class=""button-container"">
                            <a href=""{urlReset}"" class=""button"">Redefinir Senha</a>
                        </div>
                        <div class=""info-box"">
                            <p><strong>Dicas de Segurança:</strong></p>
                            <p>Nunca compartilhe sua senha com ninguém.</p>
                            <p>Use senhas fortes e únicas para cada serviço.</p>
                        </div>
                        <p style=""color: #999; font-size: 13px; text-align: center;"">Este link expira em 1 hora por razões de segurança.</p>
                        <div class=""divider""></div>
                        <p style=""font-size: 14px; color: #666;"">Se você não solicitou essa alteração, por favor ignore este e-mail.</p>
                        <p style=""font-size: 14px; color: #666;"">Dúvidas? <a href=""mailto:squadhackathonio@gmail.com"" style=""color: #27ae60; text-decoration: none;"">Entre em contato conosco</a></p>
                    </div>
                    <div class=""footer"">
                        <p class=""footer-text""><strong>PlantaCore</strong> &copy; 2026 - Seu app de plantas inteligente</p>
                        <p class=""footer-text"">Segurança e privacidade em primeiro lugar</p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
