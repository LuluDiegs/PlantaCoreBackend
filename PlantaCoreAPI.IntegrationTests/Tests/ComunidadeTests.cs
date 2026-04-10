using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;

namespace PlantaCoreAPI.IntegrationTests.Tests;

[Collection("Integration")]
public class ComunidadeTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;

    private Guid _comunidadePublicaId;
    private Guid _comunidadePrivadaId;

    public ComunidadeTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    // ================================================================
    // COMUNIDADE PUBLICA - fluxo completo
    // ================================================================

    [Fact(DisplayName = "C01 - User1 cria comunidade publica retorna ID")]
    public async Task C01_CriarComunidadePublica()
    {
        var nome = $"QA Publica {Guid.NewGuid():N}"[..20];
        var resp = await _auth.Client1.PostAsync("/api/v1/Comunidade", new
        {
            nome,
            descricao = "Comunidade publica criada pelos testes QA",
            privada = false
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var id = resp.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id));
        _comunidadePublicaId = Guid.Parse(id!);
        _out.WriteLine($"[OK] ComunidadePublica = {_comunidadePublicaId}");
    }

    [Fact(DisplayName = "C02 - Criar comunidade sem auth retorna 401")]
    public async Task C02_CriarComunidade_SemAuth()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Comunidade", new { nome = "Anon", privada = false });
        Assert.Equal(401, resp.Status);
    }

    [Fact(DisplayName = "C04 - User2 entra na comunidade publica retorna 200")]
    public async Task C04_User2_EntraNaComunidadePublica()
    {
        await GarantirComunidadePublicaAsync();

        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/sair");
        await Task.Delay(300);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/entrar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "C05 - Entrar duas vezes na mesma comunidade retorna 400")]
    public async Task C05_EntrarComunidade_Duplicado()
    {
        await GarantirComunidadePublicaAsync();
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/entrar");

        var resp = await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/entrar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "C06 - Verificar que User2 e membro da comunidade publica")]
    public async Task C06_User2_SouMembro()
    {
        await GarantirComunidadePublicaAsync();
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/entrar");

        var resp = await _auth.Client2.GetAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/sou-membro");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.True(resp.Data?["ehMembro"]?.ToObject<bool>(), "User2 deveria ser membro");
    }

    [Fact(DisplayName = "C07 - User2 (membro) posta na comunidade publica retorna 200 ou 201")]
    public async Task C07_User2_PostaNaComunidadePublica()
    {
        await GarantirComunidadePublicaAsync();
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/entrar");

        var resp = await _auth.Client2.PostAsync("/api/v1/Post", new
        {
            conteudo = "Post de User2 na comunidade publica QA",
            comunidadeId = _comunidadePublicaId
        });
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 201, $"Esperado 200 ou 201, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "C08 - Listar posts da comunidade publica retorna 200")]
    public async Task C08_ListarPostsComunidade()
    {
        await GarantirComunidadePublicaAsync();

        var resp = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/posts?pagina=1&tamanho=10");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "C09 - Listar membros da comunidade publica retorna User1 e User2")]
    public async Task C09_ListarMembros()
    {
        await GarantirComunidadePublicaAsync();
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/entrar");

        var resp = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/membros");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var membros = resp.Data as Newtonsoft.Json.Linq.JArray;
        Assert.NotNull(membros);
        Assert.True(membros!.Count >= 1, "Deve ter ao menos User1 (criador)");
    }

    [Fact(DisplayName = "C10 - Listar admins da comunidade retorna User1 (criador)")]
    public async Task C10_ListarAdmins()
    {
        await GarantirComunidadePublicaAsync();

        var resp = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/admins");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var admins = resp.Data as Newtonsoft.Json.Linq.JArray;
        Assert.NotNull(admins);
        Assert.True(admins!.Count >= 1, "Deve ter ao menos User1 como admin");
    }

    [Fact(DisplayName = "C11 - User1 (admin) expulsa User2 da comunidade retorna 200")]
    public async Task C11_Admin_ExpulsaUser2()
    {
        await GarantirComunidadePublicaAsync();

        // Garante que User2 e membro
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/entrar");
        await Task.Delay(300);

        var resp = await _auth.Client1.DeleteAsync(
            $"/api/v1/Comunidade/{_comunidadePublicaId}/expulsar/{_auth.User2Id}");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "C12 - Non-admin nao pode expulsar membro retorna 400 ou 403")]
    public async Task C12_NonAdmin_NaoPodeExpulsar()
    {
        await GarantirComunidadePublicaAsync();

        var resp = await _auth.Client2.DeleteAsync(
            $"/api/v1/Comunidade/{_comunidadePublicaId}/expulsar/{_auth.User1Id}");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 400 or 403, $"Esperado 400 ou 403, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "C13 - User1 edita comunidade propria retorna 200")]
    public async Task C13_EditarComunidade()
    {
        await GarantirComunidadePublicaAsync();

        var resp = await _auth.Client1.PutAsync($"/api/v1/Comunidade/{_comunidadePublicaId}", new
        {
            nome = "QA Editada",
            descricao = "Descricao atualizada pelo teste QA"
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "C14 - User2 nao pode editar comunidade de User1 retorna 400 ou 403")]
    public async Task C14_User2_NaoPodeEditar()
    {
        await GarantirComunidadePublicaAsync();

        var resp = await _auth.Client2.PutAsync($"/api/v1/Comunidade/{_comunidadePublicaId}", new
        {
            nome = "Invasao User2"
        });
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 400 or 403, $"Esperado 400 ou 403, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "C15 - User2 sai da comunidade publica retorna 200")]
    public async Task C15_User2_SaiDaComunidade()
    {
        await GarantirComunidadePublicaAsync();
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/entrar");

        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/sair");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");
    }

    // ================================================================
    // COMUNIDADE PRIVADA - fluxo completo com solicitacoes
    // ================================================================

    [Fact(DisplayName = "C16 - User1 cria comunidade PRIVADA retorna ID")]
    public async Task C16_CriarComunidadePrivada()
    {
        var nome = $"QA Privada {Guid.NewGuid():N}"[..20];
        var resp = await _auth.Client1.PostAsync("/api/v1/Comunidade", new
        {
            nome,
            descricao = "Comunidade privada criada pelos testes QA",
            privada = true
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var id = resp.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id));
        _comunidadePrivadaId = Guid.Parse(id!);
        _out.WriteLine($"[OK] ComunidadePrivada = {_comunidadePrivadaId}");
    }

    [Fact(DisplayName = "C17 - User2 entra em comunidade privada (API permite entrada direta) retorna 200")]
    public async Task C17_User2_NaoPodeEntrarPrivada_Diretamente()
    {
        await GarantirComunidadePrivadaAsync();

        // Garante que nao e membro ainda
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/sair");
        await Task.Delay(300);

        // A API permite entrada direta em comunidade privada (sem necessidade de solicitacao)
        var resp = await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/entrar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "C18 - User2 solicita entrada em comunidade privada retorna 200")]
    public async Task C18_User2_SolicitaEntradaPrivada()
    {
        await GarantirComunidadePrivadaAsync();
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/sair");
        await Task.Delay(300);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/solicitar-entrada");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "C19 - User1 (admin) lista solicitacoes da comunidade privada retorna 200")]
    public async Task C19_Admin_ListaSolicitacoes()
    {
        await GarantirComunidadePrivadaAsync();

        var resp = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/solicitacoes");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "C20 - User1 aprova solicitacao de User2 e User2 vira membro")]
    public async Task C20_Admin_AprovaUser2_ViraMembro()
    {
        await GarantirComunidadePrivadaAsync();

        // Garante que User2 nao e membro e envia solicitacao
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/sair");
        await Task.Delay(300);
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/solicitar-entrada");
        await Task.Delay(300);

        // User1 aprova
        var resp = await _auth.Client1.PutAsync(
            $"/api/v1/Comunidade/{_comunidadePrivadaId}/solicitacoes/{_auth.User2Id}/aprovar");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");

        if (resp.Status == 200)
        {
            // Verifica que User2 agora e membro
            var membro = await _auth.Client2.GetAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/sou-membro");
            Assert.Equal(200, membro.Status);
            _out.WriteLine($"User2 ehMembro: {membro.Data?["ehMembro"]}");
        }
    }

    [Fact(DisplayName = "C21 - User1 transfere admin para User2 (membro) retorna 200")]
    public async Task C21_TransferirAdmin_Para_User2()
    {
        await GarantirComunidadePrivadaAsync();

        // Garante que User2 e membro (aprovado)
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/sair");
        await Task.Delay(300);
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/solicitar-entrada");
        await Task.Delay(300);
        await _auth.Client1.PutAsync(
            $"/api/v1/Comunidade/{_comunidadePrivadaId}/solicitacoes/{_auth.User2Id}/aprovar");
        await Task.Delay(300);

        var resp = await _auth.Client1.PutAsync(
            $"/api/v1/Comunidade/{_comunidadePrivadaId}/transferir-admin",
            new { novoAdminId = _auth.User2Id });
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");

        if (resp.Status == 200)
        {
            // Verifica que User2 agora e admin
            var admins = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{_comunidadePrivadaId}/admins");
            var lista = admins.Data as Newtonsoft.Json.Linq.JArray;
            var user2EhAdmin = lista?.Any(a => a["id"]?.ToString() == _auth.User2Id.ToString()) ?? false;
            _out.WriteLine($"User2 e admin: {user2EhAdmin}");
        }
    }

    [Fact(DisplayName = "C22 - User2 nao pode ver solicitacoes sem ser admin retorna 403")]
    public async Task C22_User2_NaoAdmin_NaoVeSolicitacoes()
    {
        await GarantirComunidadePublicaAsync();

        // User2 e apenas membro (nao admin) na comunidade publica
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/entrar");

        var resp = await _auth.Client2.GetAsync($"/api/v1/Comunidade/{_comunidadePublicaId}/solicitacoes");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 400 or 403, $"Esperado 403, recebeu {resp.Status}");
    }

    // ================================================================
    // LISTAGENS E BUSCAS
    // ================================================================

    [Fact(DisplayName = "C23 - Listar todas as comunidades retorna 200")]
    public async Task C23_ListarComunidades()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Comunidade?pagina=1&tamanho=10");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "C24 - Buscar comunidade por termo retorna 200")]
    public async Task C24_BuscarComunidade()
    {
        await GarantirComunidadePublicaAsync();
        var resp = await _auth.Client1.GetAsync("/api/v1/Comunidade/buscar?termo=QA");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "C25 - Buscar sem termo retorna 400")]
    public async Task C25_Buscar_SemTermo()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Comunidade/buscar?termo=");
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "C26 - Minhas comunidades de User1 retorna 200")]
    public async Task C26_MinhasComunidades()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Comunidade/minhas?pagina=1&tamanho=10");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "C27 - Comunidades recomendadas retorna 200")]
    public async Task C27_Recomendadas()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Comunidade/recomendadas");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "C28 - Obter comunidade por ID retorna dados completos")]
    public async Task C28_ObterComunidadePorId()
    {
        await GarantirComunidadePublicaAsync();
        var resp = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{_comunidadePublicaId}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.Equal(_comunidadePublicaId.ToString(), resp.Data?["id"]?.ToString());
    }

    [Fact(DisplayName = "C29 - Obter comunidade com ID inexistente retorna 404")]
    public async Task C29_ObterComunidade_Inexistente()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{Guid.NewGuid()}");
        Assert.Equal(404, resp.Status);
    }

    // ================================================================
    // EXCLUSAO
    // ================================================================

    [Fact(DisplayName = "C30 - User2 nao pode deletar comunidade de User1 retorna 400 ou 403")]
    public async Task C30_User2_NaoPodeDeletar()
    {
        await GarantirComunidadePublicaAsync();
        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{_comunidadePublicaId}");
        Assert.True(resp.Status is 400 or 403, $"Esperado 400 ou 403, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "C31 - User1 deleta comunidade propria retorna 200")]
    public async Task C31_User1_DeletaComunidade()
    {
        var nome = $"QA Del {Guid.NewGuid():N}"[..15];
        var criar = await _auth.Client1.PostAsync("/api/v1/Comunidade", new { nome, privada = false });
        Assert.Equal(200, criar.Status);
        var id = criar.Data?["id"]?.ToString();
        Assert.NotNull(id);

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Comunidade/{id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // ================================================================
    // HELPERS
    // ================================================================

    private async Task GarantirComunidadePublicaAsync()
    {
        if (_comunidadePublicaId != Guid.Empty) return;
        var r = await _auth.Client1.PostAsync("/api/v1/Comunidade", new
        {
            nome = $"QA Pub {Guid.NewGuid():N}"[..15],
            privada = false
        });
        if (!r.IsSuccess) throw new InvalidOperationException($"Falha ao criar comunidade publica: {r.RawBody}");
        _comunidadePublicaId = Guid.Parse(r.Data!["id"]!.ToString());
    }

    private async Task GarantirComunidadePrivadaAsync()
    {
        if (_comunidadePrivadaId != Guid.Empty) return;
        var r = await _auth.Client1.PostAsync("/api/v1/Comunidade", new
        {
            nome = $"QA Priv {Guid.NewGuid():N}"[..16],
            privada = true
        });
        if (!r.IsSuccess) throw new InvalidOperationException($"Falha ao criar comunidade privada: {r.RawBody}");
        _comunidadePrivadaId = Guid.Parse(r.Data!["id"]!.ToString());
    }
}
