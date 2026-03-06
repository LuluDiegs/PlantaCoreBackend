namespace PlantaCoreAPI.Infrastructure.Storage;

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
    private const string BucketFotos = "fotos";

    public SupabaseFileStorageService(HttpClient httpClient, string supabaseUrl, string supabaseKey)
    {
        _httpClient = httpClient;
        _supabaseUrl = supabaseUrl.TrimEnd('/');
        _supabaseKey = supabaseKey;
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
            var stringContent = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            request.Content = stringContent;

            Console.WriteLine($"ListarTodosArquivosAsync - URL: {url}");
            Console.WriteLine($"ListarTodosArquivosAsync - Body: {jsonBody}");

            var response = await _httpClient.SendAsync(request);

            Console.WriteLine($"ListarTodosArquivosAsync - Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ListarTodosArquivosAsync - Erro: {errorContent}");
                return new List<string>();
            }

            var json = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"ListarTodosArquivosAsync - Response: {json}");

            if (string.IsNullOrWhiteSpace(json) || json == "[]")
                return new List<string>();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var arquivos = JsonSerializer.Deserialize<List<ArquivoSupabase>>(json, options) ?? new List<ArquivoSupabase>();

            var urls = new List<string>();
            foreach (var arquivo in arquivos)
            {
                if (arquivo != null && !string.IsNullOrWhiteSpace(arquivo.Name))
                {
                    var urlPublica = $"{_supabaseUrl}/storage/v1/object/public/{BucketFotos}/{arquivo.Name}";
                    urls.Add(urlPublica);
                }
            }

            return urls;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ListarTodosArquivosAsync Exception: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
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

            Console.WriteLine($"FazerUploadAsync - URL: {url}");
            Console.WriteLine($"FazerUploadAsync - Nome: {nomeUnico}");
            Console.WriteLine($"FazerUploadAsync - Tamanho: {bytes.Length} bytes");

            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(tipoConteudo);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            request.Headers.Add("apikey", _supabaseKey);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");

            var response = await _httpClient.SendAsync(request);

            Console.WriteLine($"FazerUploadAsync - Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var urlPublica = $"{_supabaseUrl}/storage/v1/object/public/{BucketFotos}/{nomeUnico}";
                Console.WriteLine($"FazerUploadAsync - Sucesso: {urlPublica}");
                return urlPublica;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"FazerUploadAsync - Erro: {errorContent}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FazerUploadAsync Exception: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
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

            Console.WriteLine($"FazerUploadFotoPerfilAsync - URL: {url}");

            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            request.Headers.Add("apikey", _supabaseKey);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");

            var response = await _httpClient.SendAsync(request);

            Console.WriteLine($"FazerUploadFotoPerfilAsync - Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var urlPublica = $"{_supabaseUrl}/storage/v1/object/public/{BucketFotos}/{nomeUnico}";
                Console.WriteLine($"FazerUploadFotoPerfilAsync - Sucesso: {urlPublica}");
                return urlPublica;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"FazerUploadFotoPerfilAsync - Erro: {errorContent}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FazerUploadFotoPerfilAsync Exception: {ex.Message}");
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

            Console.WriteLine($"FazerUploadFotoPlantaAsync - URL: {url}");

            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            request.Headers.Add("apikey", _supabaseKey);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");

            var response = await _httpClient.SendAsync(request);

            Console.WriteLine($"FazerUploadFotoPlantaAsync - Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var urlPublica = $"{_supabaseUrl}/storage/v1/object/public/{BucketFotos}/{nomeUnico}";
                Console.WriteLine($"FazerUploadFotoPlantaAsync - Sucesso: {urlPublica}");
                return urlPublica;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"FazerUploadFotoPlantaAsync - Erro: {errorContent}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FazerUploadFotoPlantaAsync Exception: {ex.Message}");
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
            Console.WriteLine($"ExcluirArquivoAsync Exception: {ex.Message}");
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
            Console.WriteLine($"DeletarFotoPerfilAsync Exception: {ex.Message}");
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
            Console.WriteLine($"DeletarFotoPlantaAsync Exception: {ex.Message}");
            return false;
        }
    }

    private static string ExtrairNomeArquivoDoUrl(string urlFoto)
    {
        if (string.IsNullOrWhiteSpace(urlFoto))
            return string.Empty;

        var uri = new Uri(urlFoto);
        var partes = uri.AbsolutePath.Split('/');
        return partes.Length > 0 ? partes[partes.Length - 1] : string.Empty;
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
            Console.WriteLine($"ExcluirTodosArquivosAsync Exception: {ex.Message}");
            return 0;
        }
    }
}
