namespace PlantaCoreAPI.Application.DTOs.Planta;

public class AtualizarLocalizacaoDTO
{
    public bool CompartilharLocalizacao { get; set; } = false;
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
}