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
    Task<Resultado<IEnumerable<PostDTOSaida>>> ObterFeedAsync(Guid usuarioId, int pagina = 1, int tamanho = 10);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> ListarPostsUsuarioAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> ObterExploradorAsync(Guid usuarioAutenticadoId, int pagina, int tamanho);
    Task<Resultado<IEnumerable<PostDTOSaida>>> ListarPostsCurtidosAsync(Guid usuarioId, Guid usuarioAutenticadoId);
    Task<Resultado> CurtirPostAsync(Guid usuarioId, Guid postId);
    Task<Resultado> RemoverCurtidaAsync(Guid usuarioId, Guid postId);
    Task<Resultado> CurtirComentarioAsync(Guid usuarioId, Guid comentarioId);
    Task<Resultado> RemoverCurtidaComentarioAsync(Guid usuarioId, Guid comentarioId);
    Task<Resultado<ComentarioDTOSaida>> CriarComentarioAsync(Guid usuarioId, CriarComentarioDTOEntrada entrada);
    Task<Resultado<ComentarioDTOSaida>> AtualizarComentarioAsync(Guid usuarioId, Guid comentarioId, AtualizarComentarioDTOEntrada entrada);
    Task<Resultado> ExcluirComentarioAsync(Guid usuarioId, Guid comentarioId);
    Task<Resultado> ExcluirComentarioComoDonoPostAsync(Guid donoPostId, Guid comentarioId);
    Task<Resultado<IEnumerable<ComentarioDTOSaida>>> ListarComentariosPostAsync(Guid postId, Guid usuarioAutenticadoId, int pagina = 1, int tamanho = 20);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> ObterFeedFiltradoAsync(Guid usuarioId, string ordenacao, int pagina, int tamanho);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> BuscarPostsPorPlantaAsync(string nomePlanta, int pagina, int tamanho);
    Task<Resultado<IEnumerable<PostDTOSaida>>> BuscarPostsPorHashtagAsync(string hashtag);
    Task<Resultado<IEnumerable<PostDTOSaida>>> BuscarPostsPorCategoriaAsync(string categoria);
    Task<Resultado<IEnumerable<PostDTOSaida>>> BuscarPostsPorPalavraChaveAsync(string palavraChave);
}
