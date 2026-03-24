using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Notificacao;

namespace PlantaCoreAPI.Application.Interfaces;

public interface INotificationService
{
    Task<Resultado<PaginaResultadoNotificacao>> ObterNotificacoesPaginadasAsync(Guid usuarioId, int pagina, int tamanho);
    Task<Resultado> MarcarComoLidaAsync(Guid notificacaoId);
    Task<Resultado> MarcarTodasComoLidasAsync(Guid usuarioId);
    Task<Resultado> DeletarNotificacaoAsync(Guid notificacaoId, Guid usuarioId);
    Task<Resultado> DeletarTodasNotificacoesAsync(Guid usuarioId);
    Task<Resultado<ConfiguracoesNotificacaoDTOSaida>> ObterConfiguracoesAsync(Guid usuarioId);
    Task<Resultado> AtualizarConfiguracoesAsync(Guid usuarioId, ConfiguracoesNotificacaoDTOEntrada entrada);
}

public class PaginaResultadoNotificacao
{
    public IEnumerable<NotificacaoDTOSaida> Notificacoes { get; set; } = new List<NotificacaoDTOSaida>();
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalNaoLidas { get; set; }
}
