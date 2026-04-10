using System.Net;
using System.Net.Mail;

using Microsoft.Extensions.Logging;

using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Utils;

namespace PlantaCoreAPI.Infrastructure.Services.External;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _emailRemetente;
    private readonly string _senhaRemetente;
    private readonly ILogger<EmailService> _logger;

    public EmailService(string smtpHost, int smtpPort, string emailRemetente, string senhaRemetente, ILogger<EmailService> logger)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _emailRemetente = emailRemetente;
        _senhaRemetente = senhaRemetente;
        _logger = logger;
    }

    public async Task<bool> EnviarAsync(string destinatario, string assunto, string corpo)
    {
        try
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_emailRemetente, _senhaRemetente);

            var mensagem = new MailMessage(_emailRemetente, destinatario, assunto, corpo)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mensagem);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {Destinatario}", EmailMascarador.Mascarar(destinatario));
            return false;
        }
    }
}
