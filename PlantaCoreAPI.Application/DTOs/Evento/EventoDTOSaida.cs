namespace PlantaCoreAPI.Application.DTOs.Evento;

public class EventoDTOSaida
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public Guid AnfitriaoId { get; set; }
    public List<Guid> ParticipantesIds { get; set; } = new();
    public int TotalParticipantes { get; set; }
}
