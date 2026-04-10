using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;

namespace PlantaCoreAPI.IntegrationTests.Tests;

[Collection("Integration")]
public class NotificacaoTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;
    private Guid _notificacaoId;

    public NotificacaoTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    [Fact(DisplayName = "N01 - Listar notificacoes paginadas retorna 200")]
    public async Task N01_ListarNotificacoes()
    {
        var post = await _auth.Client1.PostAsync("/api/v1/Post", new
        {
            conteudo = $"Post para gerar notificacao QA {Guid.NewGuid():N}"[..45]
        });
        var postId = post.Data?["id"]?.ToString();
        if (!string.IsNullOrEmpty(postId))
            await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/curtir");

        var resp = await _auth.Client1.GetAsync("/api/v1/Notificacao?pagina=1&tamanho=20");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        CapturarIdNotificacao(resp);
    }

    [Fact(DisplayName = "N02 - Notificacoes sem auth retorna 401")]
    public async Task N02_Notificacoes_SemAuth()
    {
        var resp = await _auth.Anon.GetAsync("/api/v1/Notificacao");
        Assert.Equal(401, resp.Status);
    }

    [Fact(DisplayName = "N03 - Notificacoes nao lidas retorna 200")]
    public async Task N03_NaoLidas()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Notificacao/nao-lidas");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        CapturarIdNotificacao(resp);
    }

    [Fact(DisplayName = "N04 - Marcar todas como lidas retorna 200")]
    public async Task N04_MarcarTodasComoLidas()
    {
        var resp = await _auth.Client1.PutAsync("/api/v1/Notificacao/marcar-todas-como-lidas");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "N05 - Marcar notificacao individual como lida retorna 200 ou 400")]
    public async Task N05_MarcarIndividual()
    {
        await GarantirNotificacaoAsync();
        if (_notificacaoId == Guid.Empty) { _out.WriteLine("[SKIP]"); return; }

        var resp = await _auth.Client1.PutAsync($"/api/v1/Notificacao/{_notificacaoId}/marcar-como-lida");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "N06 - Marcar notificacao inexistente retorna 400 ou 404")]
    public async Task N06_Marcar_Inexistente()
    {
        var resp = await _auth.Client1.PutAsync($"/api/v1/Notificacao/{Guid.NewGuid()}/marcar-como-lida");
        Assert.True(resp.Status is 400 or 404, $"Esperado 400 ou 404, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "N07 - Deletar notificacao individual retorna 200 ou 400")]
    public async Task N07_DeletarNotificacao()
    {
        await GarantirNotificacaoAsync();
        if (_notificacaoId == Guid.Empty) { _out.WriteLine("[SKIP]"); return; }

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Notificacao/{_notificacaoId}");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");
        _notificacaoId = Guid.Empty;
    }

    [Fact(DisplayName = "N08 - Obter configuracoes de notificacao retorna 200")]
    public async Task N08_ObterConfiguracoes()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Notificacao/configuracoes");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.NotNull(resp.Data);
    }

    [Fact(DisplayName = "N09 - Atualizar configuracoes de notificacao retorna 200")]
    public async Task N09_AtualizarConfiguracoes()
    {
        var resp = await _auth.Client1.PutAsync("/api/v1/Notificacao/configuracoes", new
        {
            receberCurtidas = true,
            receberComentarios = true,
            receberNovoSeguidor = true,
            receberSolicitacaoSeguir = true,
            receberSolicitacaoAceita = true,
            receberEvento = true,
            receberPlantaCuidado = true,
            receberPlantaIdentificada = true
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "N10 - Configuracoes sem auth retorna 401")]
    public async Task N10_Configuracoes_SemAuth()
    {
        var resp = await _auth.Anon.GetAsync("/api/v1/Notificacao/configuracoes");
        Assert.Equal(401, resp.Status);
    }

    [Fact(DisplayName = "N11 - Deletar todas as notificacoes de User2 retorna 200")]
    public async Task N11_DeletarTodas()
    {
        var resp = await _auth.Client2.DeleteAsync("/api/v1/Notificacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "L01 - Gerar lembretes de cuidado para todas as plantas retorna 200")]
    public async Task L01_GerarLembretes()
    {
        var resp = await _auth.Client1.PostAsync("/api/v1/lembretes-cuidado/gerar-para-todas-plantas");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "L02 - Gerar lembretes sem auth retorna 401")]
    public async Task L02_GerarLembretes_SemAuth()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/lembretes-cuidado/gerar-para-todas-plantas");
        Assert.Equal(401, resp.Status);
    }

    private async Task GarantirNotificacaoAsync()
    {
        if (_notificacaoId != Guid.Empty) return;
        var resp = await _auth.Client1.GetAsync("/api/v1/Notificacao?pagina=1&tamanho=20");
        CapturarIdNotificacao(resp);
    }

    private void CapturarIdNotificacao(ApiResponse resp)
    {
        if (_notificacaoId != Guid.Empty) return;
        try
        {
            var arr = resp.Data as Newtonsoft.Json.Linq.JArray
                   ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
            if (arr?.Count > 0 && Guid.TryParse(arr[0]["id"]?.ToString(), out var g))
                _notificacaoId = g;
        }
        catch { }
    }
}
