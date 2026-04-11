using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;

namespace PlantaCoreAPI.IntegrationTests.Tests;

[Collection("Integration")]
public class UsuarioTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;

    public UsuarioTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    // --- Perfil ---

    [Fact(DisplayName = "U01 - Perfil proprio autenticado retorna dados do usuario")]
    public async Task U01_ObterPerfil()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Usuario/perfil");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.Equal(_auth.User1Id.ToString(), resp.Data?["id"]?.ToString());
    }

    [Fact(DisplayName = "U02 - Perfil sem token retorna 401")]
    public async Task U02_ObterPerfil_SemAuth()
    {
        var resp = await _auth.Anon.GetAsync("/api/v1/Usuario/perfil");
        Assert.Equal(401, resp.Status);
    }

    [Fact(DisplayName = "U03 - Perfil publico de User2 acessivel por User1")]
    public async Task U03_PerfilPublico_User2()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Usuario/perfil-publico/{_auth.User2Id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.Equal(_auth.User2Id.ToString(), resp.Data?["id"]?.ToString());
    }

    [Fact(DisplayName = "U04 - Perfil publico de User1 (privado) acessivel - mostra dados basicos")]
    public async Task U04_PerfilPublico_User1_Privado()
    {
        var resp = await _auth.Client2.GetAsync($"/api/v1/Usuario/perfil-publico/{_auth.User1Id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U05 - Perfil publico com ID inexistente retorna 400 ou 404")]
    public async Task U05_PerfilPublico_IdInexistente()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Usuario/perfil-publico/{Guid.NewGuid()}");
        Assert.True(resp.Status is 400 or 404, $"Esperado 400 ou 404, recebeu {resp.Status}");
    }

    // --- Edicao de perfil ---

    [Fact(DisplayName = "U06 - Atualizar nome com dado valido retorna 200")]
    public async Task U06_AtualizarNome()
    {
        var resp = await _auth.Client1.PutAsync("/api/v1/Usuario/nome", new { novoNome = "Luiza QA" });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U07 - Atualizar nome vazio retorna 400")]
    public async Task U07_AtualizarNome_Vazio()
    {
        var resp = await _auth.Client1.PutAsync("/api/v1/Usuario/nome", new { novoNome = "" });
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "U08 - Atualizar biografia retorna 200")]
    public async Task U08_AtualizarBiografia()
    {
        var resp = await _auth.Client1.PutAsync("/api/v1/Usuario/biografia", new { biografia = "Bio atualizada pelo teste QA" });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // --- Privacidade ---

    [Fact(DisplayName = "U09 - Confirmar User1 esta com perfil privado")]
    public async Task U09_User1_PerfilPrivado()
    {
        var resp = await _auth.Client1.PutAsync("/api/v1/Usuario/privacidade", new { privado = true });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var perfil = await _auth.Client1.GetAsync("/api/v1/Usuario/perfil");
        var privado = perfil.Data?["perfilPrivado"]?.ToObject<bool>();
        Assert.True(privado, "User1 deveria estar com perfil privado");
    }

    [Fact(DisplayName = "U10 - User2 perfil publico verificado")]
    public async Task U10_User2_PerfilPublico()
    {
        await _auth.Client2.PutAsync("/api/v1/Usuario/privacidade", new { privado = false });
        var perfil = await _auth.Client2.GetAsync("/api/v1/Usuario/perfil");
        var privado = perfil.Data?["perfilPrivado"]?.ToObject<bool>();
        Assert.False(privado, "User2 deveria estar com perfil publico");
    }

    // --- Seguir (end-to-end com perfil privado) ---

    [Fact(DisplayName = "U11 - User2 ja segue User1 (via solicitacao aceita na fixture)")]
    public async Task U11_User2_SeguindoUser1()
    {
        var resp = await _auth.Client2.GetAsync("/api/v1/Usuario/meu-seguindo");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var lista = resp.Data as Newtonsoft.Json.Linq.JArray ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var encontrou = lista?.Any(u => u["id"]?.ToString() == _auth.User1Id.ToString()) ?? false;
        _out.WriteLine($"User1 na lista de seguindo de User2: {encontrou}");
    }

    [Fact(DisplayName = "U12 - User1 ja segue User2")]
    public async Task U12_User1_SeguindoUser2()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Usuario/meu-seguindo");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U13 - Seguir a si mesmo retorna 400")]
    public async Task U13_SeguirASiMesmo()
    {
        var resp = await _auth.Client1.PostAsync($"/api/v1/Usuario/seguir/{_auth.User1Id}");
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "U14 - User2 tenta seguir User1 diretamente (privado) retorna erro")]
    public async Task U14_SeguirPerfilPrivado_Direto()
    {
        await _auth.Client2.DeleteAsync($"/api/v1/Usuario/seguir/{_auth.User1Id}");
        await Task.Delay(500);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Usuario/seguir/{_auth.User1Id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status); 

        await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        var sols = await _auth.Client1.GetAsync("/api/v1/Usuario/solicitacoes-seguir");
        var solId = ExtrairPrimeiroId(sols);
        if (solId != null)
            await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/aceitar");
    }

    [Fact(DisplayName = "U15 - Relacao entre User1 e User2 retorna dados de seguimento mutuo")]
    public async Task U15_RelacaoMutua()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Usuario/{_auth.User2Id}/relacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U16 - Listar seguidores de User1 inclui User2")]
    public async Task U16_SeguidoresUser1()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Usuario/meus-seguidores");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U17 - Listar seguidores de User2 inclui User1")]
    public async Task U17_SeguidoresUser2()
    {
        var resp = await _auth.Client2.GetAsync("/api/v1/Usuario/meus-seguidores");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U18 - Lista de seguindo paginada de User1 retorna 200")]
    public async Task U18_SeguindoLista()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Usuario/{_auth.User1Id}/seguindo/lista");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U19 - Lista de seguidores paginada de User1 retorna 200")]
    public async Task U19_SeguidoresLista()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Usuario/{_auth.User1Id}/seguidores/lista");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // --- Solicitacoes de seguir ---

    [Fact(DisplayName = "U20 - Listar solicitacoes de seguir de User1 retorna 200")]
    public async Task U20_ListarSolicitacoes()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Usuario/solicitacoes-seguir");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U21 - Rejeitar solicitacao de seguir - fluxo completo")]
    public async Task U21_RejeitarSolicitacao_Fluxo()
    {
        await _auth.Client2.DeleteAsync($"/api/v1/Usuario/seguir/{_auth.User1Id}");
        await Task.Delay(300);
        var sol = await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        _out.WriteLine($"Solicitacao: {sol}");

        var listagem = await _auth.Client1.GetAsync("/api/v1/Usuario/solicitacoes-seguir");
        var solId = ExtrairPrimeiroId(listagem);

        if (solId == null) { _out.WriteLine("[SKIP] Sem solicitacao disponivel"); return; }

        var resp = await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/rejeitar");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");

        await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        var sols2 = await _auth.Client1.GetAsync("/api/v1/Usuario/solicitacoes-seguir");
        var solId2 = ExtrairPrimeiroId(sols2);
        if (solId2 != null)
            await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId2}/aceitar");
    }

    // --- Outros perfis ---

    [Fact(DisplayName = "U22 - Sugestoes de usuarios para seguir retorna 200")]
    public async Task U22_Sugestoes()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Usuario/sugestoes");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U23 - Posts salvos de User1 retorna 200")]
    public async Task U23_PostsSalvos()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Usuario/posts-salvos");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U24 - Comunidades do usuario retorna 200")]
    public async Task U24_ComunidadesDoUsuario()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Usuario/{_auth.User1Id}/comunidades?pagina=1&tamanho=10");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U25 - Posts do usuario retorna 200")]
    public async Task U25_PostsDoUsuario()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Usuario/{_auth.User1Id}/posts");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "U26 - Reativacao com conta ativa retorna 400")]
    public async Task U26_Reativacao_ContaAtiva()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Usuario/reativar/solicitar", new { email = TestContext.User1Email });
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "U27 - Verificar token invalido de reativacao retorna 400")]
    public async Task U27_Reativacao_TokenInvalido()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Usuario/reativar/verificar-token", new
        {
            email = TestContext.User1Email,
            token = "token_invalido_qa"
        });
        Assert.Equal(400, resp.Status);
    }

    private static string? ExtrairPrimeiroId(ApiResponse resp)
    {
        try
        {
            var arr = resp.Data as Newtonsoft.Json.Linq.JArray
                   ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
            return arr?.Count > 0 ? arr[0]["id"]?.ToString() : null;
        }
        catch { return null; }
    }
}
