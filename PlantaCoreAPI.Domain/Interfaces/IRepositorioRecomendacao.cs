using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioRecomendacao
{
    Task<IEnumerable<Recomendacao>> ObterTodosAsync();
    Task<IEnumerable<Recomendacao>> ObterPorUsuarioAsync(Guid usuarioId);
    Task AdicionarAsync(Recomendacao recomendacao);
}