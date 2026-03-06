using System.Security.Claims;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IJwtService
{
    string GerarTokenAcesso(Guid usuarioId, string email, string nome);
    string GerarTokenRefresh();
    ClaimsPrincipal? ValidarTokenAcesso(string token);
}
