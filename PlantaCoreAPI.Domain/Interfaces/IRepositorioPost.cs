using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioPost : IRepositorio<Entities.Post>
{
    Task<IEnumerable<Entities.Post>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<PaginaResultado<Entities.Post>> ObterPorUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho);
    Task<IEnumerable<Entities.Post>> ObterFeedAsync(Guid usuarioId, int pagina = 1, int tamanho = 10);
    Task<PaginaResultado<Entities.Post>> ObterExploradorAsync(int pagina, int tamanho);
    Task<IEnumerable<Entities.Post>> ObterPorPlantaAsync(Guid plantaId);
    Task<PaginaResultado<Entities.Post>> ObterPorComunidadeAsync(Guid comunidadeId, int pagina, int tamanho);
    Task<Entities.Post?> ObterPorComentarioIdAsync(Guid comentarioId);
    Task<IEnumerable<Entities.Post>> ObterPostsCurtidosPorUsuarioAsync(Guid usuarioId);
    Task<int> ObterTotalCurtidasRecebidasAsync(Guid usuarioId);
    Task<Entities.Comentario?> ObterComentarioPorIdAsync(Guid comentarioId);
    Task AtualizarComentarioAsync(Entities.Comentario comentario);
    Task<IEnumerable<Entities.Post>> ObterPorIdsAsync(IEnumerable<Guid> postIds);
    Task<IEnumerable<Entities.Post>> ObterPorHashtagAsync(string hashtag);
    Task<IEnumerable<Entities.Post>> ObterPorCategoriaAsync(string categoria);
    Task<IEnumerable<Entities.Post>> ObterPorPalavraChaveAsync(string palavraChave);
}
