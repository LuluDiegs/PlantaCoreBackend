namespace PlantaCoreAPI.IntegrationTests.Infrastructure;

public class SharedAuthFixture : IAsyncLifetime
{
    public ApiClient Client1 { get; } = new(TestContext.BaseUrl);
    public ApiClient Client2 { get; } = new(TestContext.BaseUrl);
    public ApiClient Anon { get; } = new(TestContext.BaseUrl);
    public Guid User1Id => Client1.UserId!.Value;
    public Guid User2Id => Client2.UserId!.Value;

    public async Task InitializeAsync()
    {
        var ok1 = await Client1.LoginAsync(TestContext.User1Email, TestContext.User1Senha);
        if (!ok1) throw new InvalidOperationException(
            $"Falha no login de User1 ({TestContext.User1Email}). API rodando em {TestContext.BaseUrl}?");

        await Task.Delay(800);

        var ok2 = await Client2.LoginAsync(TestContext.User2Email, TestContext.User2Senha);
        if (!ok2) throw new InvalidOperationException(
            $"Falha no login de User2 ({TestContext.User2Email}).");

        await Client1.PutAsync("/api/v1/Usuario/privacidade", new { privado = true });

        await Client2.DeleteAsync($"/api/v1/Usuario/seguir/{User1Id}");
        await Task.Delay(300);
        var seguir2Para1 = await Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{User1Id}");
        if (seguir2Para1.IsSuccess)
        {
            var sols = await Client1.GetAsync("/api/v1/Usuario/solicitacoes-seguir");
            var solId = ExtrairIdPorUsuario(sols, User2Id);
            if (solId != null)
                await Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/aceitar");
        }

        await Client1.DeleteAsync($"/api/v1/Usuario/seguir/{User2Id}");
        await Task.Delay(300);
        await Client1.PostAsync($"/api/v1/Usuario/seguir/{User2Id}");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static string? ExtrairIdPorUsuario(ApiResponse resp, Guid usuarioId)
    {
        try
        {
            var arr = (resp.Data as Newtonsoft.Json.Linq.JArray)
                   ?? (resp.Json?["data"] as Newtonsoft.Json.Linq.JArray);
            if (arr == null) return null;
            foreach (var item in arr)
            {
                var uid = item["usuarioId"]?.ToString() ?? item["solicitanteId"]?.ToString();
                if (uid == usuarioId.ToString())
                    return item["id"]?.ToString();
            }
            return arr.Count > 0 ? arr[0]["id"]?.ToString() : null;
        }
        catch { return null; }
    }
}
