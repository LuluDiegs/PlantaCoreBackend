using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioEvento : IRepositorio<Evento>
{
    Task<Evento?> ObterPorTituloAsync(string titulo);
    Task<List<Evento>> ObterTodosComParticipantesAsync();
    Task<List<Usuario>> ListarParticipantesAsync(Guid eventoId);
}
