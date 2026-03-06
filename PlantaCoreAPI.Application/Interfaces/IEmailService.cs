namespace PlantaCoreAPI.Application.Interfaces;

public interface IEmailService
{
    Task<bool> EnviarAsync(string destinatario, string assunto, string corpo);
}
