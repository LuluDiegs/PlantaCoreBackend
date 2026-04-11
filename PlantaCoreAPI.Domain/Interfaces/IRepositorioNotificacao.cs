using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioNotificacao : IRepositorio<Entities.Notificacao>
{
    Task<IEnumerable<Entities.Notificacao>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<PaginaResultado<Entities.Notificacao>> ObterPorUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho);
    Task<IEnumerable<Entities.Notificacao>> ObterNaoLidasAsync(Guid usuarioId);
    Task<int> ContarNaoLidasAsync(Guid usuarioId);
    Task<bool> ExisteLembreteHojeAsync(Guid plantaId);
    Task MarcarComoLidaAsync(Guid notificacaoId);
    Task MarcarTodasComoLidasAsync(Guid usuarioId);
    Task DeletarNotificacaoAsync(Guid notificacaoId, Guid usuarioId);
    Task DeletarTodasNotificacoesUsuarioAsync(Guid usuarioId);
    Task DeletarTodasDoUsuarioAsync(Guid usuarioId);
}
