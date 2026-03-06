using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Notificacao;

namespace PlantaCoreAPI.Application.Interfaces;

public interface INotificationService
{
    Task<Resultado<ListarNotificacoesComLembretesDTOSaida>> ObterNotificacoesAsync(Guid usuarioId);
    Task<Resultado<ListarNotificacoesComLembretesDTOSaida>> ObterNaoLidasAsync(Guid usuarioId);
    Task<Resultado> MarcarComoLidaAsync(Guid notificacaoId);
    Task<Resultado> MarcarTodasComoLidasAsync(Guid usuarioId);
    Task<Resultado> DeletarNotificacaoAsync(Guid notificacaoId, Guid usuarioId);
    Task<Resultado> DeletarTodasNotificacoesAsync(Guid usuarioId);
}
