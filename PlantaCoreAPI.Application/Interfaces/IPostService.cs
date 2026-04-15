using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Comentario;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IPostService
{
    Task<Resultado<PostDTOSaida>> CriarPostAsync(Guid usuarioId, CriarPostDTOEntrada entrada);
    Task<Resultado<PostDTOSaida>> AtualizarPostAsync(Guid usuarioId, Guid postId, AtualizarPostDTOEntrada entrada);
    Task<Resultado> ExcluirPostAsync(Guid usuarioId, Guid postId);
    Task<Resultado<PostDTOSaida>> ObterPostAsync(Guid postId, Guid usuarioId);
    Task<Resultado<IEnumerable<PostDTOSaida>>> ObterFeedAsync(Guid usuarioId, int pagina = 1, int tamanho = 10, string? ordenarPor = null);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> ListarPostsUsuarioAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho, string? ordenarPor);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> ObterExploradorAsync(Guid usuarioAutenticadoId, int pagina, int tamanho, string? ordenarPor);
    Task<Resultado<IEnumerable<PostDTOSaida>>> ListarPostsCurtidosAsync(Guid usuarioId, Guid usuarioAutenticadoId);
    Task<Resultado> CurtirPostAsync(Guid usuarioId, Guid postId);
    Task<Resultado> RemoverCurtidaAsync(Guid usuarioId, Guid postId);
    Task<Resultado> CurtirComentarioAsync(Guid usuarioId, Guid comentarioId);
    Task<Resultado> RemoverCurtidaComentarioAsync(Guid usuarioId, Guid comentarioId);
    Task<Resultado<ComentarioDTOSaida>> CriarComentarioAsync(Guid usuarioId, CriarComentarioDTOEntrada entrada);
    Task<Resultado<ComentarioDTOSaida>> AtualizarComentarioAsync(Guid usuarioId, Guid comentarioId, AtualizarComentarioDTOEntrada entrada);
    Task<Resultado> ExcluirComentarioAsync(Guid usuarioId, Guid comentarioId);
    Task<Resultado> ExcluirComentarioComoDonoPostAsync(Guid donoPostId, Guid comentarioId);
    Task<Resultado<IEnumerable<ComentarioDTOSaida>>> ListarComentariosPostAsync(Guid postId, Guid usuarioAutenticadoId, int pagina = 1, int tamanho = 20, string? ordenar = null);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> BuscarPostsPorPlantaAsync(string nomePlanta, int pagina, int tamanho);
    Task<IEnumerable<PostDTOSaida>> ObterTrendingPostsAsync(int quantidade = 10);
    Task<Resultado> SalvarPostAsync(Guid usuarioId, Guid postId);
    Task<Resultado> RemoverPostSalvoAsync(Guid usuarioId, Guid postId);
    Task<Resultado<IEnumerable<PostDTOSaida>>> ListarPostsSalvosAsync(Guid usuarioId);
    Task<Resultado> CompartilharPostAsync(Guid usuarioId, Guid postId);
    Task<Resultado> VisualizarPostAsync(Guid usuarioId, Guid postId);
    Task<Resultado<ComentarioDTOSaida>> ResponderComentarioAsync(Guid usuarioId, Guid comentarioId, string conteudo);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> ListarPostsComunidadeAsync(Guid comunidadeId, Guid usuarioId, int pagina, int tamanho, string? ordenarPor);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> BuscarPostsAsync(string? q, int pagina, int tamanho);
}
