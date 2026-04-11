using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services.External;

public class PlantNetService : IPlantNetService
{
    private readonly HttpClient _httpClient;
    private readonly string _chaveApi;
    private readonly ILogger<PlantNetService> _logger;
    private const string BaseUrl = "https://my-api.plantnet.org/v2/identify/all";

    public PlantNetService(HttpClient httpClient, string chaveApi, ILogger<PlantNetService> logger)
    {
        _httpClient = httpClient;
        _chaveApi = chaveApi;
        _logger = logger;
    }

    public async Task<ResultadoIdentificacaoPlantNet?> IdentificarPlantaPorArquivoAsync(string caminhoArquivo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_chaveApi) || !File.Exists(caminhoArquivo))
                return null;
            using var fileStream = File.OpenRead(caminhoArquivo);
            using var multipartContent = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            multipartContent.Add(streamContent, "images", Path.GetFileName(caminhoArquivo));
            multipartContent.Add(new StringContent("auto"), "organs");
            var url = $"{BaseUrl}?api-key={Uri.EscapeDataString(_chaveApi)}&lang=pt";
            var response = await _httpClient.PostAsync(url, multipartContent);
            if (!response.IsSuccessStatusCode)
                return null;
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
                return null;
            return Deserializar(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[PlantNet] Erro ao identificar planta por arquivo {Arquivo}", caminhoArquivo);
            return null;
        }
    }

    public async Task<ResultadoIdentificacaoPlantNet?> IdentificarPlantaPorUrlAsync(string urlImagem)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_chaveApi) || string.IsNullOrWhiteSpace(urlImagem))
                return null;
            using var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(new StringContent(urlImagem), "images");
            multipartContent.Add(new StringContent("auto"), "organs");
            var url = $"{BaseUrl}?api-key={Uri.EscapeDataString(_chaveApi)}&lang=pt";
            var response = await _httpClient.PostAsync(url, multipartContent);
            if (!response.IsSuccessStatusCode)
                return null;
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
                return null;
            return Deserializar(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[PlantNet] Erro ao identificar planta por URL");
            return null;
        }
    }

    private static ResultadoIdentificacaoPlantNet? Deserializar(string json)
    {
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var raw = System.Text.Json.JsonSerializer.Deserialize<PlantNetResponseRaw>(json, options);
        if (raw == null) return null;
        return new ResultadoIdentificacaoPlantNet
        {
            Query = raw.Query == null ? null : new QueryPlantNet
            {
                Data = raw.Query.Data,
                Imagens = raw.Query.Imagens
            },
            Resultados = raw.Results?.Select(r => new ResultadoPlantNet
            {
                Probabilidade = r.Probability,
                Score = r.Score,
                Especie = r.Species == null ? null : new EspeciePlantNet
                {
                    NomeCientifico = r.Species.ScientificName,
                    NomesComuns = r.Species.CommonNames
                }
            }).ToList() ?? new()
        };
    }
}

internal sealed class PlantNetResponseRaw
{
    [JsonPropertyName("results")]
    public List<PlantNetResultRaw>? Results { get; set; }

    [JsonPropertyName("query")]
    public PlantNetQueryRaw? Query { get; set; }
}

internal sealed class PlantNetQueryRaw
{
    [JsonPropertyName("date")]
    public string? Data { get; set; }

    [JsonPropertyName("images")]
    public List<string>? Imagens { get; set; }
}

internal sealed class PlantNetResultRaw
{
    [JsonPropertyName("species")]
    public PlantNetSpeciesRaw? Species { get; set; }

    [JsonPropertyName("probability")]
    public double Probability { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }
}

internal sealed class PlantNetSpeciesRaw
{
    [JsonPropertyName("scientificName")]
    public string? ScientificName { get; set; }

    [JsonPropertyName("commonNames")]
    public List<string>? CommonNames { get; set; }
}
