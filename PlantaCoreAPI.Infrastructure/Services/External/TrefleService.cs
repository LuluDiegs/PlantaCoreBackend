using System.Text.Json.Serialization;
using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services.External;

public class TrefleService : ITrefleService
{
    private readonly HttpClient _httpClient;
    private readonly string _chaveApi;
    private const string BaseUrl = "https://trefle.io/api/v1";

    public TrefleService(HttpClient httpClient, string chaveApi)
    {
        _httpClient = httpClient;
        _chaveApi = chaveApi;
    }

    public async Task<PlantaTrefle?> ObterPlantaPorIdAsync(int plantaId)
    {
        try
        {
            if (plantaId <= 0)
                return null;

            var url = $"{BaseUrl}/plants/{plantaId}?token={_chaveApi}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
                return null;

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var wrapper = System.Text.Json.JsonSerializer.Deserialize<TreflePlantaDetalheRaw>(json, options);
            var raw = wrapper?.Data;

            return raw == null ? null : MapearParaPlantaTrefle(raw);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ResultadoBuscaTrefle?> BuscarPlantasAsync(string termo, int pagina = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(termo))
                return null;

            if (pagina < 0)
                pagina = 0;

            var termoEscapado = Uri.EscapeDataString(termo.Trim());
            var url = $"{BaseUrl}/plants/search?q={termoEscapado}&page={pagina}&token={_chaveApi}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
                return null;

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var raw = System.Text.Json.JsonSerializer.Deserialize<TrefleBuscaRaw>(json, options);
            if (raw == null) return null;

            var resultado = new ResultadoBuscaTrefle
            {
                Dados = raw.Data?.Select(p => MapearParaPlantaTrefle(p)).ToList() ?? new(),
                Metadados = raw.Meta == null ? null : new MetadadosTrefle
                {
                    Total = raw.Meta.Total,
                    TotalPaginas = raw.Meta.TotalPages,
                    Pagina = raw.Meta.CurrentPage
                },
                Links = raw.Links == null ? null : new LinksTrefle
                {
                    Primeiro = raw.Links.First,
                    Ultimo = raw.Links.Last,
                    Proximo = raw.Links.Next
                }
            };

            if (!resultado.Dados.Any() && termo.Contains(" "))
            {
                var primeiroNome = termo.Substring(0, termo.IndexOf(" ")).Trim();
                if (!string.IsNullOrWhiteSpace(primeiroNome))
                    return await BuscarPlantasAsync(primeiroNome, pagina);
            }

            return resultado;
        }
        catch
        {
            return null;
        }
    }

    private static PlantaTrefle MapearParaPlantaTrefle(TreflePlantaRaw raw)
    {
        return new PlantaTrefle
        {
            Id = raw.Id,
            NomeCientifico = raw.ScientificName,
            NomeComum = TraduzirTexto(raw.CommonName),
            Slug = raw.Slug,
            UrlImagem = raw.ImageUrl,
            Genero = TraduzirTexto(raw.Genus),
            Familia = TraduzirTexto(raw.Family)
        };
    }

    private static string? TraduzirTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return texto;

        var dicionario = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "leaf", "folha" }, { "leaves", "folhas" }, { "stem", "caule" },
            { "flower", "flor" }, { "flowers", "flores" }, { "fruit", "fruto" },
            { "fruits", "frutos" }, { "seed", "semente" }, { "seeds", "sementes" },
            { "root", "raiz" }, { "roots", "raízes" }, { "tree", "árvore" },
            { "trees", "árvores" }, { "shrub", "arbusto" }, { "shrubs", "arbustos" },
            { "herb", "erva" }, { "herbs", "ervas" }, { "vine", "videira" },
            { "vines", "videiras" }, { "grass", "grama" }, { "grasses", "gramas" },
            { "annual", "anual" }, { "perennial", "perene" }, { "deciduous", "decídua" },
            { "evergreen", "perene" }, { "conifer", "conífera" }, { "coniferous", "conífero" },
            { "broadleaf", "folha larga" }, { "succulent", "suculenta" }, { "cactus", "cacto" },
            { "fern", "samambaia" }, { "moss", "musgo" }, { "fungus", "fungo" },
            { "epiphyte", "epífita" }, { "parasite", "parasita" }, { "saprophyte", "saprofita" },
            { "nitrogen fixer", "fixadora de nitrogênio" }
        };

        var resultado = texto;
        foreach (var kvp in dicionario)
        {
            resultado = System.Text.RegularExpressions.Regex.Replace(
                resultado,
                $@"\b{kvp.Key}\b",
                kvp.Value,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return resultado;
    }
}

internal sealed class TrefleMetaRaw
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }
}

internal sealed class TrefleLinksRaw
{
    [JsonPropertyName("first")]
    public string? First { get; set; }

    [JsonPropertyName("last")]
    public string? Last { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}

internal sealed class TreflePlantaDetalheRaw
{
    [JsonPropertyName("data")]
    public TreflePlantaRaw? Data { get; set; }
}

internal sealed class TrefleBuscaRaw
{
    [JsonPropertyName("data")]
    public List<TreflePlantaRaw>? Data { get; set; }

    [JsonPropertyName("meta")]
    public TrefleMetaRaw? Meta { get; set; }

    [JsonPropertyName("links")]
    public TrefleLinksRaw? Links { get; set; }
}

internal sealed class TreflePlantaRaw
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("common_name")]
    public string? CommonName { get; set; }

    [JsonPropertyName("scientific_name")]
    public string? ScientificName { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("genus")]
    public string? Genus { get; set; }

    [JsonPropertyName("family")]
    public string? Family { get; set; }
}
