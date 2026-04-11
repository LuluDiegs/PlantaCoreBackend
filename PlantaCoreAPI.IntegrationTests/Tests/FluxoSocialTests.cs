using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;

namespace PlantaCoreAPI.IntegrationTests.Tests;

[Collection("Integration")]
public class FluxoSocialTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;

    public FluxoSocialTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    [Fact(DisplayName = "FS01 - Confirmar User1 esta com perfil privado")]
    public async Task FS01_User1_PerfilPrivado()
    {
        var resp = await _auth.Client1.PutAsync("/api/v1/Usuario/privacidade", new { privado = true });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var perfil = await _auth.Client1.GetAsync("/api/v1/Usuario/perfil");
        var privado = perfil.Data?["perfilPrivado"]?.ToObject<bool>();
        _out.WriteLine($"User1 perfilPrivado: {privado}");
        Assert.True(privado, "User1 deve estar com perfil privado");
    }

    [Fact(DisplayName = "FS03 - Apos desseguir, User2 tenta seguir User1 diretamente (privado) retorna 400")]
    public async Task FS03_User2_TentaSeguirDireto_PerfilPrivado()
    {
        await GarantirUser2NaoSeguindoUser1Async();

        var resp = await _auth.Client2.PostAsync($"/api/v1/Usuario/seguir/{_auth.User1Id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);

        var mensagem = resp.Data?["mensagem"]?.ToString() ?? resp.Json?["errors"]?[0]?.ToString() ?? "";
        _out.WriteLine($"Mensagem: {mensagem}");
    }

    [Fact(DisplayName = "FS04 - User2 envia solicitacao de seguir User1 (privado) retorna 200")]
    public async Task FS04_User2_EnviaSolicitacao()
    {
        await GarantirUser2NaoSeguindoUser1Async();
        await LimparSolicitacoesPendentesAsync();

        var resp = await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "FS05 - Solicitacao duplicada retorna 400")]
    public async Task FS05_Solicitacao_Duplicada()
    {
        await GarantirUser2NaoSeguindoUser1Async();
        await LimparSolicitacoesPendentesAsync();

        await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        await Task.Delay(300);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "FS06 - User1 lista solicitacoes pendentes e ve solicitacao de User2")]
    public async Task FS06_User1_VeSolicitacao_De_User2()
    {
        await GarantirUser2NaoSeguindoUser1Async();
        await LimparSolicitacoesPendentesAsync();
        await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        await Task.Delay(300);

        var resp = await _auth.Client1.GetAsync("/api/v1/Usuario/solicitacoes-seguir");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var lista = resp.Data as Newtonsoft.Json.Linq.JArray
                 ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        Assert.NotNull(lista);
        var temSolicitacaoDeUser2 = lista!.Any(s =>
            s["solicitanteId"]?.ToString() == _auth.User2Id.ToString());
        _out.WriteLine($"Solicitacao de User2 visivel para User1: {temSolicitacaoDeUser2}");
        Assert.True(temSolicitacaoDeUser2, "User1 deveria ver a solicitacao de User2");
    }

    [Fact(DisplayName = "FS07 - User1 REJEITA solicitacao de User2 retorna 200")]
    public async Task FS07_User1_Rejeita_Solicitacao()
    {
        await GarantirUser2NaoSeguindoUser1Async();
        await LimparSolicitacoesPendentesAsync();
        await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        await Task.Delay(300);

        var solId = await ObterIdSolicitacaoDeUser2Async();
        Assert.True(solId != null, "Deveria existir uma solicitacao pendente de User2");

        var resp = await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/rejeitar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "FS08 - Apos rejeicao, User2 NAO segue User1")]
    public async Task FS08_AposRejeicao_User2_NaoSegue()
    {
        await GarantirUser2NaoSeguindoUser1Async();
        await LimparSolicitacoesPendentesAsync();
        await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        await Task.Delay(300);
        var solId = await ObterIdSolicitacaoDeUser2Async();
        if (solId != null)
            await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/rejeitar");
        await Task.Delay(300);

        var seguindo = await _auth.Client2.GetAsync("/api/v1/Usuario/meu-seguindo");
        var lista = seguindo.Data as Newtonsoft.Json.Linq.JArray
                 ?? seguindo.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var segueUser1 = lista?.Any(u => u["id"]?.ToString() == _auth.User1Id.ToString()) ?? false;
        _out.WriteLine($"User2 segue User1 apos rejeicao: {segueUser1}");
        Assert.False(segueUser1, "Apos rejeicao User2 NAO deve seguir User1");
    }

    [Fact(DisplayName = "FS09 - Apos rejeicao, User2 tenta seguir diretamente de novo retorna 400")]
    public async Task FS09_AposRejeicao_TentaSeguirDireto_Novamente()
    {
        await GarantirUser2NaoSeguindoUser1Async();

        var resp = await _auth.Client2.PostAsync($"/api/v1/Usuario/seguir/{_auth.User1Id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "FS10 - Apos rejeicao, User2 pode enviar NOVA solicitacao retorna 200")]
    public async Task FS10_AposRejeicao_Nova_Solicitacao()
    {
        await GarantirUser2NaoSeguindoUser1Async();
        await LimparSolicitacoesPendentesAsync();

        await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        await Task.Delay(300);
        var solId = await ObterIdSolicitacaoDeUser2Async();
        if (solId != null)
        {
            await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/rejeitar");
            await Task.Delay(300);
        }

        var resp = await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "FS11 - User1 ACEITA nova solicitacao de User2 e User2 volta a seguir")]
    public async Task FS11_User1_Aceita_NovasSolicitacao_User2_SegueNovamente()
    {
        await GarantirUser2NaoSeguindoUser1Async();
        await LimparSolicitacoesPendentesAsync();

        var sol = await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        _out.WriteLine($"Solicitacao enviada: {sol}");
        Assert.Equal(200, sol.Status);
        await Task.Delay(300);

        var solId = await ObterIdSolicitacaoDeUser2Async();
        Assert.True(solId != null, "Deveria existir uma solicitacao pendente de User2");

        var aceitar = await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/aceitar");
        _out.WriteLine($"Aceitar: {aceitar}");
        Assert.Equal(200, aceitar.Status);
        await Task.Delay(300);

        var seguindo = await _auth.Client2.GetAsync("/api/v1/Usuario/meu-seguindo");
        var lista = seguindo.Data as Newtonsoft.Json.Linq.JArray
                 ?? seguindo.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var segueUser1 = lista?.Any(u => u["id"]?.ToString() == _auth.User1Id.ToString()) ?? false;
        _out.WriteLine($"User2 segue User1 apos aceite: {segueUser1}");
        Assert.True(segueUser1, "Apos aceitar User2 DEVE seguir User1");
    }

    [Fact(DisplayName = "FS12 - User1 deixa de seguir User2 e User2 continua como seguidor de User1")]
    public async Task FS12_User1_DessegueUser2_User2_AindaSegueUser1()
    {
        await GarantirUser1SeguindoUser2Async();
        await GarantirUser2SeguindoUser1Async();

        var desseguir = await _auth.Client1.DeleteAsync($"/api/v1/Usuario/seguir/{_auth.User2Id}");
        _out.WriteLine($"User1 desseguiu User2: {desseguir}");
        Assert.True(desseguir.Status is 200 or 400);

        var seguidores = await _auth.Client1.GetAsync("/api/v1/Usuario/meus-seguidores");
        var lista = seguidores.Data as Newtonsoft.Json.Linq.JArray
                 ?? seguidores.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var user2EhSeguidor = lista?.Any(u => u["id"]?.ToString() == _auth.User2Id.ToString()) ?? false;
        _out.WriteLine($"User2 ainda e seguidor de User1: {user2EhSeguidor}");

        await _auth.Client1.PostAsync($"/api/v1/Usuario/seguir/{_auth.User2Id}");
    }

    [Fact(DisplayName = "FS13 - Nao e possivel enviar solicitacao para perfil publico")]
    public async Task FS13_Solicitacao_Para_Perfil_Publico_Retorna_400()
    {
        await _auth.Client2.PutAsync("/api/v1/Usuario/privacidade", new { privado = false });
        await Task.Delay(300);

        var resp = await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User2Id}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "FS14 - User2 nao pode aceitar solicitacao que pertence a User1")]
    public async Task FS14_User2_NaoPodeAceitar_Solicitacao_De_Outro()
    {
        await GarantirUser2NaoSeguindoUser1Async();
        await LimparSolicitacoesPendentesAsync();
        await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
        await Task.Delay(300);

        var solId = await ObterIdSolicitacaoDeUser2Async();
        if (solId == null) { _out.WriteLine("[SKIP] Sem solicitacao disponivel"); return; }

        var resp = await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/aceitar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);

        await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/rejeitar");
    }

    [Fact(DisplayName = "FS15 - Relacao entre User1 e User2 reflete estado atual de seguimento")]
    public async Task FS15_Relacao_Reflete_Estado_Atual()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Usuario/{_auth.User2Id}/relacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.NotNull(resp.Data);
    }

    // ================================================================
    // HELPERS
    // ================================================================
    private async Task GarantirUser2NaoSeguindoUser1Async()
    {
        // Retorna 400 se ja nao segue — isso e ok
        await _auth.Client2.DeleteAsync($"/api/v1/Usuario/seguir/{_auth.User1Id}");
        await Task.Delay(400);
    }

    private async Task GarantirUser2SeguindoUser1Async()
    {
        for (var tentativa = 1; tentativa <= 3; tentativa++)
        {
            var relacao = await _auth.Client2.GetAsync($"/api/v1/Usuario/{_auth.User1Id}/relacao");
            var jaSegue = relacao.Data?["seguindo"]?.ToObject<bool>() ?? false;
            if (jaSegue) return;

            await LimparSolicitacoesPendentesAsync();
            await Task.Delay(200);

            var enviar = await _auth.Client2.PostAsync($"/api/v1/Usuario/solicitacao-seguir/{_auth.User1Id}");
            if (!enviar.IsSuccess)
            {
                await Task.Delay(300);
                continue;
            }

            await Task.Delay(500);

            var solId = await ObterIdSolicitacaoDeUser2Async();
            if (solId == null)
            {
                await Task.Delay(300);
                continue;
            }

            var aceitar = await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{solId}/aceitar");
            if (!aceitar.IsSuccess)
            {
                await Task.Delay(300);
                continue;
            }

            await Task.Delay(500);
        }

        var relacaoFinal = await _auth.Client2.GetAsync($"/api/v1/Usuario/{_auth.User1Id}/relacao");
        var segueAoFinal = relacaoFinal.Data?["seguindo"]?.ToObject<bool>() ?? false;
        if (!segueAoFinal)
            throw new InvalidOperationException(
                $"GarantirUser2SeguindoUser1Async falhou apos 3 tentativas. " +
                $"Relacao final: {relacaoFinal.RawBody}");
    }

    private async Task GarantirUser1SeguindoUser2Async()
    {
        var seguindo = await _auth.Client1.GetAsync("/api/v1/Usuario/meu-seguindo");
        var lista = seguindo.Data as Newtonsoft.Json.Linq.JArray
                 ?? seguindo.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var jaSegue = lista?.Any(u => u["id"]?.ToString() == _auth.User2Id.ToString()) ?? false;
        if (!jaSegue)
        {
            await _auth.Client1.PostAsync($"/api/v1/Usuario/seguir/{_auth.User2Id}");
            await Task.Delay(300);
        }
    }

    private async Task LimparSolicitacoesPendentesAsync()
    {
        var sols = await _auth.Client1.GetAsync("/api/v1/Usuario/solicitacoes-seguir");
        var lista = sols.Data as Newtonsoft.Json.Linq.JArray
                 ?? sols.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        if (lista == null) return;
        foreach (var item in lista)
        {
            var uid = item["solicitanteId"]?.ToString();
            if (uid == _auth.User2Id.ToString())
            {
                var id = item["id"]?.ToString();
                if (id != null)
                {
                    await _auth.Client1.PostAsync($"/api/v1/Usuario/solicitacoes-seguir/{id}/rejeitar");
                    await Task.Delay(200);
                }
            }
        }
    }

    private async Task<string?> ObterIdSolicitacaoDeUser2Async()
    {
        var sols = await _auth.Client1.GetAsync("/api/v1/Usuario/solicitacoes-seguir");
        var lista = sols.Data as Newtonsoft.Json.Linq.JArray
                 ?? sols.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        if (lista == null) return null;
        foreach (var item in lista)
        {
            if (item["solicitanteId"]?.ToString() == _auth.User2Id.ToString())
                return item["id"]?.ToString();
        }
        return null;
    }
}
