using System.Net;
using Newtonsoft.Json.Linq;

namespace PlantaCoreAPI.IntegrationTests.Infrastructure;

public class ApiResponse
{
    public HttpStatusCode StatusCode { get; private set; }
    public int Status => (int)StatusCode;
    public bool IsSuccess => Status is >= 200 and < 300;
    public string RawBody { get; private set; } = string.Empty;
    public JObject? Json { get; private set; }
    public JToken? Data { get; private set; }
    public JToken? Meta { get; private set; }
    public string? ErrorMessage { get; private set; }
    public static Task<ApiResponse> FromBodyAsync(HttpResponseMessage msg, string body)
    {
        var r = new ApiResponse { StatusCode = msg.StatusCode, RawBody = body };
        r.Parse();
        return Task.FromResult(r);
    }

    public static async Task<ApiResponse> FromAsync(HttpResponseMessage msg)
    {
        var body = await msg.Content.ReadAsStringAsync();
        return await FromBodyAsync(msg, body);
    }

    private void Parse()
    {
        if (string.IsNullOrWhiteSpace(RawBody)) return;
        try
        {
            Json = JObject.Parse(RawBody);
            Data = Json["dados"] ?? Json["data"];
            Meta = Json["meta"];

            var errors = Json["erros"] ?? Json["errors"];
            if (errors is JArray arr && arr.Count > 0)
                ErrorMessage = arr[0]?.ToString();
            else
                ErrorMessage = Json["message"]?.ToString()
                            ?? Json["mensagem"]?.ToString();
        }
        catch { }
    }

    public Guid? ExtractId()
    {
        var raw = Data?["id"]?.ToString() ?? Data?.ToString();
        return Guid.TryParse(raw, out var g) ? g : null;
    }

    public override string ToString() =>
        $"HTTP {Status} | {RawBody[..Math.Min(300, RawBody.Length)]}";
}
