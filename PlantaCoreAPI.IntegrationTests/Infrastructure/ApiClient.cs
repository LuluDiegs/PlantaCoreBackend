using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace PlantaCoreAPI.IntegrationTests.Infrastructure;

public class ApiClient
{
    private readonly HttpClient _http;
    public string? Token { get; private set; }
    public Guid? UserId { get; private set; }
    public string? RefreshToken { get; private set; }

    private const int MaxRetries = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    public ApiClient(string baseUrl = "http://localhost:5123")
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(60)
        };
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void SetToken(string token)
    {
        Token = token;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        Token = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<ApiResponse> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> sendAsync)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            HttpResponseMessage httpResp = await sendAsync();
            string body = await httpResp.Content.ReadAsStringAsync();

            bool isTransient = (int)httpResp.StatusCode is 503 or 500
                || ((int)httpResp.StatusCode is 400 or 404
                    && body.Contains("transient failure", StringComparison.OrdinalIgnoreCase));

            if (!isTransient || attempt == MaxRetries - 1)
                return await ApiResponse.FromBodyAsync(httpResp, body);

            await Task.Delay(RetryDelay);
        }

        throw new InvalidOperationException("Retry loop terminou inesperadamente");
    }

    public Task<ApiResponse> GetAsync(string url) =>
        ExecuteWithRetryAsync(() => _http.GetAsync(url));

    public Task<ApiResponse> PostAsync(string url, object? body = null)
    {
        return ExecuteWithRetryAsync(() =>
        {
            var content = body != null
                ? new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                : new StringContent("{}", Encoding.UTF8, "application/json");
            return _http.PostAsync(url, content);
        });
    }

    public Task<ApiResponse> PutAsync(string url, object? body = null)
    {
        return ExecuteWithRetryAsync(() =>
        {
            var content = body != null
                ? new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                : new StringContent("{}", Encoding.UTF8, "application/json");
            return _http.PutAsync(url, content);
        });
    }

    public Task<ApiResponse> DeleteAsync(string url) =>
        ExecuteWithRetryAsync(() => _http.DeleteAsync(url));

    public async Task<bool> LoginAsync(string email, string senha)
    {
        ApiResponse? resp = null;
        for (int i = 0; i < 10; i++)
        {
            resp = await PostAsync("/api/v1/Autenticacao/login", new { email, senha });
            if (resp.IsSuccess) break;
            await Task.Delay(TimeSpan.FromSeconds(3 + i));
        }
        if (resp is null || !resp.IsSuccess) return false;

        var token = resp.Data?["tokenAcesso"]?.ToString()
                 ?? resp.Data?["accessToken"]?.ToString()
                 ?? resp.Json?["dados"]?["tokenAcesso"]?.ToString()
                 ?? resp.Json?["data"]?["tokenAcesso"]?.ToString();

        var refreshToken = resp.Data?["tokenRefresh"]?.ToString()
                        ?? resp.Data?["refreshToken"]?.ToString()
                        ?? resp.Json?["dados"]?["tokenRefresh"]?.ToString();

        var userId = resp.Data?["usuarioId"]?.ToString()
                  ?? resp.Json?["dados"]?["usuarioId"]?.ToString();

        if (string.IsNullOrEmpty(token)) return false;

        SetToken(token);
        RefreshToken = refreshToken;
        if (Guid.TryParse(userId, out var uid)) UserId = uid;
        return true;
    }

    public Task<ApiResponse> PostMultipartAsync(string url, MultipartFormDataContent form)
    {
        return ExecuteWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = form
            };

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            return _http.SendAsync(request);
        });
    }
}
