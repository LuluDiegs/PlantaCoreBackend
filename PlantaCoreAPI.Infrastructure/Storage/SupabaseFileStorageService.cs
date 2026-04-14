namespace PlantaCoreAPI.Infrastructure.Storage;
using Microsoft.Extensions.Logging;

using PlantaCoreAPI.Application.Interfaces;

using System.Text.Json;
internal sealed class ArquivoSupabase
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Updated_At { get; set; } = string.Empty;
    public string Created_At { get; set; } = string.Empty;
    public string Last_Accessed_At { get; set; } = string.Empty;
    public MetadadosArquivo? Metadata { get; set; }
}

internal sealed class MetadadosArquivo
{
    public string ETag { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Mimetype { get; set; } = string.Empty;
}

public class SupabaseFileStorageService : IFileStorageService
{
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SupabaseFileStorageService> _logger;
    private const string BucketFotos = "fotos";
    public SupabaseFileStorageService(HttpClient httpClient, string supabaseUrl, string supabaseKey, ILogger<SupabaseFileStorageService> logger)
    {
        _httpClient = httpClient;
        _supabaseUrl = supabaseUrl.TrimEnd('/');
        _supabaseKey = supabaseKey;
        _logger = logger;
    }

    public async Task<List<string>> ListarTodosArquivosAsync()
    {
        try
        {
            var url = $"{_supabaseUrl}/storage/v1/object/list/{BucketFotos}";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("apikey", _supabaseKey);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            var jsonBody = "{\"prefix\":\"\",\"limit\":100}";
            request.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ListarTodosArquivos falhou com status {StatusCode}", response.StatusCode);
                return new List<string>();
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "[]")
                return new List<string>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var arquivos = JsonSerializer.Deserialize<List<ArquivoSupabase>>(json, options) ?? new List<ArquivoSupabase>();
            var urls = new List<string>();
            foreach (var arquivo in arquivos)
            {
                if (arquivo != null && !string.IsNullOrWhiteSpace(arquivo.Name))
                    urls.Add($"{_supabaseUrl}/storage/v1/object/public/{BucketFotos}/{arquivo.Name}");
            }

            return urls;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar arquivos do Supabase");
            return new List<string>();
        }
    }

    public async Task<string> FazerUploadAsync(byte[] bytes, string nomeArquivo, string tipoConteudo)
    {
        try
        {
            string extensao = Path.GetExtension(nomeArquivo);
            string nomeUnico = $"{Guid.NewGuid()}{extensao}";
            var url = $"{_supabaseUrl}/storage/v1/object/{BucketFotos}/{nomeUnico}";
            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(tipoConteudo);
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            request.Headers.Add("apikey", _supabaseKey);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return $"{_supabaseUrl}/storage/v1/object/public/{BucketFotos}/{nomeUnico}";
            _logger.LogWarning("Upload falhou com status {StatusCode} para arquivo {NomeArquivo}", response.StatusCode, nomeArquivo);
            return string.Empty;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do arquivo {NomeArquivo}", nomeArquivo);
            return string.Empty;
        }
    }

    public async Task<string> FazerUploadFotoPerfilAsync(byte[] bytes, string nomeArquivo, Guid usuarioId)
    {
        try
        {
            string extensao = Path.GetExtension(nomeArquivo);
            string nomeUnico = $"perfil-{usuarioId}-{Guid.NewGuid()}{extensao}";
            var url = $"{_supabaseUrl}/storage/v1/object/{BucketFotos}/{nomeUnico}";
            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            request.Headers.Add("apikey", _supabaseKey);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return $"{_supabaseUrl}/storage/v1/object/public/{BucketFotos}/{nomeUnico}";
            _logger.LogWarning("Upload foto perfil falhou com status {StatusCode} para usuário {UsuarioId}", response.StatusCode, usuarioId);
            return string.Empty;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload de foto de perfil do usuário {UsuarioId}", usuarioId);
            return string.Empty;
        }
    }

    public async Task<string> FazerUploadFotoPlantaAsync(byte[] bytes, string nomeArquivo, Guid usuarioId)
    {
        try
        {
            string extensao = Path.GetExtension(nomeArquivo);
            string nomeUnico = $"planta-{usuarioId}-{Guid.NewGuid()}{extensao}";
            var url = $"{_supabaseUrl}/storage/v1/object/{BucketFotos}/{nomeUnico}";
            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            request.Headers.Add("apikey", _supabaseKey);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return $"{_supabaseUrl}/storage/v1/object/public/{BucketFotos}/{nomeUnico}";
            _logger.LogWarning("Upload foto planta falhou com status {StatusCode} para usuário {UsuarioId}", response.StatusCode, usuarioId);
            return string.Empty;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload de foto de planta do usuário {UsuarioId}", usuarioId);
            return string.Empty;
        }
    }

    public async Task<bool> ExcluirArquivoAsync(string nomeArquivo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nomeArquivo))
                return false;
            var url = $"{_supabaseUrl}/storage/v1/object/{BucketFotos}";
            var body = JsonSerializer.Serialize(new { prefixes = new[] { nomeArquivo } });
            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Add("apikey", _supabaseKey);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir arquivo {NomeArquivo}", nomeArquivo);
            return false;
        }
    }

    public async Task<bool> DeletarFotoPerfilAsync(string urlFoto, Guid usuarioId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(urlFoto))
                return true;
            var nomeArquivo = ExtrairNomeArquivoDoUrl(urlFoto);
            return await ExcluirArquivoAsync(nomeArquivo);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar foto de perfil do usuário {UsuarioId}", usuarioId);
            return false;
        }
    }

    public async Task<bool> DeletarFotoPlantaAsync(string urlFoto, Guid usuarioId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(urlFoto))
                return true;
            var nomeArquivo = ExtrairNomeArquivoDoUrl(urlFoto);
            return await ExcluirArquivoAsync(nomeArquivo);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar foto de planta do usuário {UsuarioId}", usuarioId);
            return false;
        }
    }

    private static string ExtrairNomeArquivoDoUrl(string urlFoto)
    {
        if (string.IsNullOrWhiteSpace(urlFoto))
            return string.Empty;
        var uri = new Uri(urlFoto);
        var partes = uri.AbsolutePath.Split('/');
        return partes.Length > 0 ? partes[^1] : string.Empty;
    }

    public async Task<int> ExcluirTodosArquivosAsync()
    {
        try
        {
            var urlListar = $"{_supabaseUrl}/storage/v1/object/list/{BucketFotos}";
            var requestListar = new HttpRequestMessage(HttpMethod.Post, urlListar);
            requestListar.Headers.Add("apikey", _supabaseKey);
            requestListar.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            requestListar.Content = new StringContent("{\"prefix\":\"\",\"limit\":1000}", System.Text.Encoding.UTF8, "application/json");
            var responseListar = await _httpClient.SendAsync(requestListar);
            if (!responseListar.IsSuccessStatusCode)
                return 0;
            var json = await responseListar.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "[]")
                return 0;
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var arquivos = JsonSerializer.Deserialize<List<ArquivoSupabase>>(json, options) ?? new();
            var nomes = arquivos
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .Select(a => a.Name)
                .ToArray();
            if (nomes.Length == 0)
                return 0;
            var urlExcluir = $"{_supabaseUrl}/storage/v1/object/{BucketFotos}";
            var body = JsonSerializer.Serialize(new { prefixes = nomes });
            var requestExcluir = new HttpRequestMessage(HttpMethod.Delete, urlExcluir)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            };
            requestExcluir.Headers.Add("apikey", _supabaseKey);
            requestExcluir.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            var responseExcluir = await _httpClient.SendAsync(requestExcluir);
            return responseExcluir.IsSuccessStatusCode ? nomes.Length : 0;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir todos os arquivos do Supabase");
            return 0;
        }
    }
}
