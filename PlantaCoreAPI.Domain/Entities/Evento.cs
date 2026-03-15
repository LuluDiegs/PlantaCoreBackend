namespace PlantaCoreAPI.Domain.Entities;

public class Evento
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }

    public Guid AnfitriaoId { get; set; }
    public Usuario Anfitriao { get; set; } = null!;

    public List<EventoParticipante> Participantes { get; set; } = new();
}