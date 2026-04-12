using System.Text.Json.Serialization;

namespace PlantaCoreAPI.Application.DTOs.Planta;

public class RecomendacaoPlantaDTO
{
    [JsonPropertyName("nome_comum")]
    public string NomeComum { get; set; } = string.Empty;

    [JsonPropertyName("nome_cientifico")]
    public string NomeCientifico { get; set; } = string.Empty;

    [JsonPropertyName("justificativa")]
    public string Justificativa { get; set; } = string.Empty;
}