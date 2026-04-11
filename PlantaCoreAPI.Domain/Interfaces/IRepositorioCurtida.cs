namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioCurtida : IRepositorio<Entities.Curtida>
{
    Task<bool> ExisteAsync(Guid usuarioId, Guid postId);
    Task<Entities.Curtida?> ObterPorUsuarioEPostAsync(Guid usuarioId, Guid postId);
}
