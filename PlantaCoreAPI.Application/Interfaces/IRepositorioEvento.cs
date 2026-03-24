using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Application.DTOs.Usuario;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IRepositorioEvento
{
    Task<Evento?> ObterPorIdAsync(Guid id);
    Task<Evento?> ObterPorTituloAsync(string titulo);
    Task<List<Evento>> ObterTodosAsync();
    Task AdicionarAsync(Evento evento);
    void Atualizar(Evento evento);
    void Remover(Evento evento);
    Task<bool> SalvarMudancasAsync();
    Task<IEnumerable<UsuarioListaDTOSaida>> ListarParticipantesAsync(Guid eventoId);
}
