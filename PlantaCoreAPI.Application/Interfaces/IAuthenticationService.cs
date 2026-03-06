using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Auth;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IAuthenticationService
{
    Task<Resultado<LoginDTOSaida>> RegistrarAsync(RegistroDTOEntrada entrada);
    Task<Resultado<LoginDTOSaida>> LoginAsync(LoginDTOEntrada entrada);
    Task<Resultado<LoginDTOSaida>> RefreshTokenAsync(string tokenRefresh);
    Task<Resultado> LogoutAsync(Guid usuarioId);
    Task<Resultado> ConfirmarEmailAsync(ConfirmarEmailDTOEntrada entrada);
    Task<Resultado> ReenviarConfirmacaoEmailAsync(string email);
    Task<Resultado> ResetarSenhaAsync(ResetarSenhaDTOEntrada entrada);
    Task<Resultado> NovaSenhaAsync(NovaSenhaDTOEntrada entrada);
    Task<Resultado> TrocarSenhaAsync(Guid usuarioId, TrocarSenhaDTOEntrada entrada);
}
