using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioPost : IRepositorio<Entities.Post>
{
    Task<IEnumerable<Entities.Post>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<int> ContarPorUsuarioAsync(Guid usuarioId);
    Task<PaginaResultado<Entities.Post>> ObterPorUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho, string? ordenarPor);
    Task<IEnumerable<Entities.Post>> ObterFeedAsync(Guid usuarioId, int pagina = 1, int tamanho = 10);
    Task<PaginaResultado<Entities.Post>> ObterExploradorAsync(int pagina, int tamanho, string? ordenarPor);
    Task<IEnumerable<Entities.Post>> ObterPorPlantaAsync(Guid plantaId);
    Task<PaginaResultado<Entities.Post>> ObterPorComunidadeAsync(Guid comunidadeId, int pagina, int tamanho, string? ordenarPor);
    Task<Entities.Post?> ObterPorComentarioIdAsync(Guid comentarioId);
    Task<IEnumerable<Entities.Post>> ObterPostsCurtidosPorUsuarioAsync(Guid usuarioId);
    Task<int> ObterTotalCurtidasRecebidasAsync(Guid usuarioId);
    Task<Entities.Comentario?> ObterComentarioPorIdAsync(Guid comentarioId);
    Task AtualizarComentarioAsync(Entities.Comentario comentario);
    Task<IEnumerable<Entities.Post>> ObterPorIdsAsync(IEnumerable<Guid> postIds);
    Task<IEnumerable<Entities.Post>> ObterPorHashtagAsync(string hashtag);
    Task<IEnumerable<Entities.Post>> ObterPorCategoriaAsync(string categoria);
    Task<IEnumerable<Entities.Post>> ObterPorPalavraChaveAsync(string palavraChave);
    Task<PaginaResultado<Post>> BuscarPostsAsync(string? q, int pagina, int tamanho);
    Task<PaginaResultado<Post>> BuscarPostsPorPlantaAsync(string nomePlanta, int pagina, int tamanho);
    Task<IEnumerable<Post>> ObterTrendingPostsAsync(int quantidade);
}
