using System.Net;
using System.Net.Mail;
using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services.External;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _emailRemetente;
    private readonly string _senhaRemetente;

    public EmailService(string smtpHost, int smtpPort, string emailRemetente, string senhaRemetente)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _emailRemetente = emailRemetente;
        _senhaRemetente = senhaRemetente;
    }

    public async Task<bool> EnviarAsync(string destinatario, string assunto, string corpo)
    {
        try
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_emailRemetente, _senhaRemetente);

                var mensagem = new MailMessage(_emailRemetente, destinatario, assunto, corpo)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mensagem);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            return false;
        }
    }
}
