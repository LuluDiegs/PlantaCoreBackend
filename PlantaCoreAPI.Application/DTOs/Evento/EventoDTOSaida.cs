namespace PlantaCoreAPI.Application.DTOs;

public class EventoDTOSaida
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public string Localizacao { get; set; } = null!;
    public DateTime DataInicio { get; set; }
    public Guid AnfitriaoId { get; set; }
    public List<Guid> ParticipantesIds { get; set; } = new();
}