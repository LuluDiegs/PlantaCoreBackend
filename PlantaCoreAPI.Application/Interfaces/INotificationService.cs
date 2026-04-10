using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Notificacao;

namespace PlantaCoreAPI.Application.Interfaces;

public interface INotificationService
{
    Task<Resultado<PaginaResultadoNotificacao>> ObterNotificacoesPaginadasAsync(Guid usuarioId, int pagina, int tamanho);
    Task<Resultado<IEnumerable<NotificacaoDTOSaida>>> ObterNaoLidasAsync(Guid usuarioId);
    Task<Resultado> MarcarComoLidaAsync(Guid notificacaoId, Guid usuarioId);
    Task<Resultado> MarcarTodasComoLidasAsync(Guid usuarioId);
    Task<Resultado> DeletarNotificacaoAsync(Guid notificacaoId, Guid usuarioId);
    Task<Resultado> DeletarTodasNotificacoesAsync(Guid usuarioId);
    Task<Resultado<ConfiguracoesNotificacaoDTOSaida>> ObterConfiguracoesAsync(Guid usuarioId);
    Task<Resultado> AtualizarConfiguracoesAsync(Guid usuarioId, ConfiguracoesNotificacaoDTOEntrada entrada);
}
