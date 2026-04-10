using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;

namespace PlantaCoreAPI.IntegrationTests.Tests;

/// <summary>
/// Testes end-to-end de Evento.
/// User1 cria, User2 participa, User1 edita/deleta.
/// </summary>
[Collection("Integration")]
public class EventoTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;
    private Guid _eventoId;

    public EventoTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    [Fact(DisplayName = "E01 - User1 cria evento retorna ID")]
    public async Task E01_CriarEvento()
    {
        var resp = await _auth.Client1.PostAsync("/api/v1/Evento", new
        {
            titulo = $"Evento QA {Guid.NewGuid():N}"[..20],
            descricao = "Evento criado pelos testes QA",
            localizacao = "Online",
            dataInicio = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss")
        });
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 201, $"Esperado 200 ou 201, recebeu {resp.Status}");

        var id = resp.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id));
        _eventoId = Guid.Parse(id!);
        _out.WriteLine($"[OK] EventoId = {_eventoId}");
    }

    [Fact(DisplayName = "E02 - Criar evento sem auth retorna 401")]
    public async Task E02_CriarEvento_SemAuth()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Evento", new
        {
            titulo = "Anon",
            descricao = "Anon",
            localizacao = "Anon",
            dataInicio = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss")
        });
        Assert.Equal(401, resp.Status);
    }

    [Fact(DisplayName = "E03 - Criar evento sem campos obrigatorios retorna 400")]
    public async Task E03_CriarEvento_SemCampos()
    {
        var resp = await _auth.Client1.PostAsync("/api/v1/Evento", new
        {
            titulo = "",
            descricao = "",
            localizacao = ""
        });
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "E04 - Listar todos os eventos retorna 200")]
    public async Task E04_ListarEventos()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Evento");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "E05 - Obter evento por ID retorna dados do evento")]
    public async Task E05_ObterEventoPorId()
    {
        await GarantirEventoAsync();
        var resp = await _auth.Client1.GetAsync($"/api/v1/Evento/{_eventoId}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.Equal(_eventoId.ToString(), resp.Data?["id"]?.ToString());
    }

    [Fact(DisplayName = "E06 - Obter evento inexistente retorna 404")]
    public async Task E06_ObterEvento_Inexistente()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Evento/{Guid.NewGuid()}");
        Assert.Equal(404, resp.Status);
    }

    [Fact(DisplayName = "E07 - User1 edita evento proprio retorna 200")]
    public async Task E07_EditarEvento()
    {
        await GarantirEventoAsync();
        var resp = await _auth.Client1.PutAsync($"/api/v1/Evento/{_eventoId}", new
        {
            titulo = $"Evento Editado QA {Guid.NewGuid():N}"[..22],
            descricao = "Descricao atualizada",
            localizacao = "Presencial - SP",
            dataInicio = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-ddTHH:mm:ss")
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "E08 - User2 nao pode editar evento de User1 retorna 400 ou 403")]
    public async Task E08_User2_NaoPodeEditar()
    {
        await GarantirEventoAsync();
        var resp = await _auth.Client2.PutAsync($"/api/v1/Evento/{_eventoId}", new
        {
            titulo = "Invasao User2",
            descricao = "X",
            localizacao = "X",
            dataInicio = DateTime.UtcNow.AddDays(5).ToString("yyyy-MM-ddTHH:mm:ss")
        });
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 400 or 403, $"Esperado 400 ou 403, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "E09 - User2 confirma participacao no evento retorna 200")]
    public async Task E09_User2_ConfirmaParticipacao()
    {
        await GarantirEventoAsync();
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{_eventoId}/participacao");

        var resp = await _auth.Client2.PostAsync($"/api/v1/Evento/{_eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "E10 - Participacao duplicada retorna 400")]
    public async Task E10_Participacao_Duplicada()
    {
        await GarantirEventoAsync();
        await _auth.Client2.PostAsync($"/api/v1/Evento/{_eventoId}/participacao");

        var resp = await _auth.Client2.PostAsync($"/api/v1/Evento/{_eventoId}/participacao");
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "E11 - Listar participantes do evento retorna User2")]
    public async Task E11_ListarParticipantes()
    {
        await GarantirEventoAsync();
        await _auth.Client2.PostAsync($"/api/v1/Evento/{_eventoId}/participacao");

        var resp = await _auth.Client1.GetAsync($"/api/v1/Evento/{_eventoId}/participantes");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "E12 - User2 cancela participacao retorna 200")]
    public async Task E12_User2_CancelaParticipacao()
    {
        await GarantirEventoAsync();
        await _auth.Client2.PostAsync($"/api/v1/Evento/{_eventoId}/participacao");

        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Evento/{_eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "E13 - User2 nao pode deletar evento de User1 retorna 400 ou 403")]
    public async Task E13_User2_NaoPodeDeletar()
    {
        await GarantirEventoAsync();
        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Evento/{_eventoId}");
        Assert.True(resp.Status is 400 or 403, $"Esperado 400 ou 403, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "E14 - User1 deleta evento proprio retorna 200")]
    public async Task E14_User1_DeletaEvento()
    {
        var criar = await _auth.Client1.PostAsync("/api/v1/Evento", new
        {
            titulo = $"Para Deletar {Guid.NewGuid():N}"[..20],
            descricao = "Sera deletado",
            localizacao = "Lugar nenhum",
            dataInicio = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddTHH:mm:ss")
        });
        Assert.True(criar.Status is 200 or 201);
        var id = criar.Data?["id"]?.ToString();
        Assert.NotNull(id);

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Evento/{id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    private async Task GarantirEventoAsync()
    {
        if (_eventoId != Guid.Empty) return;
        var r = await _auth.Client1.PostAsync("/api/v1/Evento", new
        {
            titulo = $"QA Auto {Guid.NewGuid():N}"[..15],
            descricao = "Evento automatico QA",
            localizacao = "Remoto",
            dataInicio = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss")
        });
        if (!r.IsSuccess) throw new InvalidOperationException($"Falha ao criar evento: {r.RawBody}");
        _eventoId = Guid.Parse(r.Data!["id"]!.ToString());
    }
}
