using PlantaCoreAPI.Application.Comuns;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IAccountReactivationService
{
    Task<Resultado> SolicitarReativacaoAsync(string email);
    Task<Resultado> ReativarComTokenAsync(string email, string token, string novaSenha);
    Task<Resultado> VerificarTokenReativacaoAsync(string email, string token);
}
