using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Evento;
using PlantaCoreAPI.Application.DTOs.Usuario;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IEventoService
{
    Task<Resultado<List<EventoDTOSaida>>> ObterEventosAsync();
    Task<Resultado<EventoDTOSaida>> ObterEventoPorIdAsync(Guid id);
    Task<Resultado<Guid>> AdicionarEventoAsync(CriarEventoDTO eventoDTO, Guid anfitriaoId);
    Task<Resultado> MarcarParticipacaoEvento(Guid eventoId, Guid usuarioId);
    Task<Resultado> DesmarcarParticipacaoEvento(Guid eventoId, Guid usuarioId);
    Task<Resultado> AtualizarEvento(Guid id, AtualizarEventoDTO eventoDTO, Guid usuarioId);
    Task<Resultado> RemoverEvento(Guid eventoId, Guid usuarioId);
    Task<Resultado<IEnumerable<UsuarioListaDTOSaida>>> ListarParticipantesAsync(Guid eventoId);
}
