namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioExclusaoConta
{
    Task DeletarFotosPlantasAsync(Guid usuarioId);
    Task DeletarSeguidoresAsync(Guid usuarioId);
    Task DeletarCurtidasAsync(Guid usuarioId);
    Task DeletarComentariosAsync(Guid usuarioId);
    Task DeletarPostsAsync(Guid usuarioId);
    Task DeletarPlantasAsync(Guid usuarioId);
    Task DeletarTokensRefreshAsync(Guid usuarioId);
    Task<IEnumerable<string?>> ObterFotosDasPlantasAsync(Guid usuarioId);
}
