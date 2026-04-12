using System.Text.Json.Serialization;

namespace PlantaCoreAPI.Application.DTOs.Planta;

public class RecomendacaoPlantaComImagemDTO
{
    [JsonPropertyName("nome_comum")]
    public string NomeComum { get; set; } = string.Empty;

    [JsonPropertyName("url_imagem")]
    public string UrlImagem { get; set; } = string.Empty;

    [JsonPropertyName("justificativa")]
    public string Justificativa { get; set; } = string.Empty;
}