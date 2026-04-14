using DotNetEnv;
using Xunit.Abstractions;

namespace PlantaCoreAPI.IntegrationTests.Infrastructure;

public class TestContext
{
    static TestContext()
    {
        Env.Load(Path.Combine(AppContext.BaseDirectory, "../../../.env"));
    }

    public static string BaseUrl => Environment.GetEnvironmentVariable("TEST_API_URL") ?? throw new InvalidOperationException("TEST_API_URL não configurada");
    public static string User1Email => Environment.GetEnvironmentVariable("TEST_USER1_EMAIL") ?? throw new InvalidOperationException("TEST_USER1_EMAIL não configurada");
    public static string User1Senha => Environment.GetEnvironmentVariable("TEST_USER1_SENHA") ?? throw new InvalidOperationException("TEST_USER1_SENHA não configurada");
    public static string User2Email => Environment.GetEnvironmentVariable("TEST_USER2_EMAIL") ?? throw new InvalidOperationException("TEST_USER2_EMAIL não configurada");
    public static string User2Senha => Environment.GetEnvironmentVariable("TEST_USER2_SENHA") ?? throw new InvalidOperationException("TEST_USER2_SENHA não configurada");
    public ApiClient Client1 { get; } = new(BaseUrl);  
    public ApiClient Client2 { get; } = new(BaseUrl); 
    public ApiClient Anon { get; } = new(BaseUrl);  
    public Guid? ComunidadeId { get; set; }
    public Guid? EventoId { get; set; }
    public Guid? PostId { get; set; }
    public Guid? ComentarioId { get; set; }
    public Guid? NotificacaoId { get; set; }
    public Guid? SolicitacaoId { get; set; }
    public Guid? User1Id => Client1.UserId;
    public Guid? User2Id => Client2.UserId;

    private readonly ITestOutputHelper? _output;

    public TestContext(ITestOutputHelper? output = null)
    {
        _output = output;
    }

    public void Log(string msg) => _output?.WriteLine(msg);

    public async Task AuthenticateAllAsync()
    {
        var ok1 = await Client1.LoginAsync(User1Email, User1Senha);
        if (!ok1) throw new InvalidOperationException(
            $"Falha no login do usuário principal ({User1Email}). Verifique se a API está rodando em {BaseUrl}.");

        await Task.Delay(TimeSpan.FromSeconds(1));

        var ok2 = await Client2.LoginAsync(User2Email, User2Senha);
        if (!ok2) throw new InvalidOperationException(
            $"Falha no login do usuário secundário ({User2Email}).");

        Log($"[AUTH] User1 ID: {User1Id}");
        Log($"[AUTH] User2 ID: {User2Id}");
    }
}
