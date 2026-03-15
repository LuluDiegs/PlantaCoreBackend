namespace PlantaCoreAPI.Domain.Entities;

public class EventoParticipante
{
    public Guid EventoId { get; set; }
    public Evento Evento { get; set; } = null!;

    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
}