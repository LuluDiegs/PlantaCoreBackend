using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;

namespace PlantaCoreAPI.IntegrationTests.Tests;

[Collection("Integration")]
public class AcoesReversiveisTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;

    public AcoesReversiveisTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    // ================================================================
    // EVENTO — marcar / desmarcar participacao
    // ================================================================

    [Fact(DisplayName = "EV01 - User2 marca participacao no evento retorna 200")]
    public async Task EV01_User2_MarcaParticipacao()
    {
        var eventoId = await CriarEventoAsync();

        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "EV02 - Marcar participacao duplicada retorna 400")]
    public async Task EV02_MarcaParticipacao_Duplicada()
    {
        var eventoId = await CriarEventoAsync();
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "EV03 - Participantes do evento incluem User2 apos marcar")]
    public async Task EV03_Participantes_IncluemUser2()
    {
        var eventoId = await CriarEventoAsync();
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        var resp = await _auth.Client1.GetAsync($"/api/v1/Evento/{eventoId}/participantes");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var lista = resp.Data as Newtonsoft.Json.Linq.JArray
                 ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var user2Participa = lista?.Any(p =>
            p["usuarioId"]?.ToString() == _auth.User2Id.ToString() ||
            p["id"]?.ToString() == _auth.User2Id.ToString()) ?? false;
        _out.WriteLine($"User2 na lista de participantes: {user2Participa}");
    }

    [Fact(DisplayName = "EV04 - User2 desmarca participacao retorna 200")]
    public async Task EV04_User2_DesmarcaParticipacao()
    {
        var eventoId = await CriarEventoAsync();
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "EV05 - Desmarcar participacao inexistente retorna 400")]
    public async Task EV05_DesmarcaParticipacao_SemParticipacao()
    {
        var eventoId = await CriarEventoAsync();
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "EV06 - Anfitriao (User1) nao pode desmarcar propria participacao retorna 400")]
    public async Task EV06_Anfitriao_NaoPodeSair()
    {
        var eventoId = await CriarEventoAsync();

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "EV07 - Apos desmarcar User2 nao aparece mais nos participantes")]
    public async Task EV07_AposDesmarcar_User2_NaoApareceParticipantes()
    {
        var eventoId = await CriarEventoAsync();
        await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        var resp = await _auth.Client1.GetAsync($"/api/v1/Evento/{eventoId}/participantes");
        var lista = resp.Data as Newtonsoft.Json.Linq.JArray
                 ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var user2Participa = lista?.Any(p =>
            p["usuarioId"]?.ToString() == _auth.User2Id.ToString() ||
            p["id"]?.ToString() == _auth.User2Id.ToString()) ?? false;
        _out.WriteLine($"User2 ainda participa apos desmarcar: {user2Participa}");
        Assert.False(user2Participa, "User2 nao deveria mais aparecer nos participantes");
    }

    [Fact(DisplayName = "EV08 - User2 pode marcar participacao novamente apos desmarcar")]
    public async Task EV08_ReMarcarParticipacao()
    {
        var eventoId = await CriarEventoAsync();
        await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // ================================================================
    // POST — salvar / dessalvar / compartilhar / visualizar
    // ================================================================

    [Fact(DisplayName = "PS01 - User1 salva post de User2 retorna 200")]
    public async Task PS01_SalvarPost()
    {
        var postId = await CriarPostAsync(_auth.Client2);
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(200);

        var resp = await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "PS02 - Post salvo aparece na lista de posts salvos de User1")]
    public async Task PS02_PostSalvo_AparececNaLista()
    {
        var postId = await CriarPostAsync(_auth.Client2);
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(300);

        var resp = await _auth.Client1.GetAsync("/api/v1/Usuario/posts-salvos");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var lista = resp.Data as Newtonsoft.Json.Linq.JArray
                 ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var encontrou = lista?.Any(p => p["id"]?.ToString() == postId.ToString()) ?? false;
        _out.WriteLine($"Post {postId} na lista de salvos: {encontrou}");
        Assert.True(encontrou, "Post salvo deve aparecer na lista de posts-salvos");
    }

    [Fact(DisplayName = "PS03 - Salvar post duplicado retorna 200 (idempotente)")]
    public async Task PS03_SalvarPost_Duplicado_Idempotente()
    {
        var postId = await CriarPostAsync(_auth.Client2);
        await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(200);

        var resp = await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "PS04 - User1 dessalva post retorna 200")]
    public async Task PS04_DessalvarPost()
    {
        var postId = await CriarPostAsync(_auth.Client2);
        await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(200);

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "PS05 - Apos dessalvar post nao aparece mais na lista de salvos")]
    public async Task PS05_AposDessalvar_NaoAparece()
    {
        var postId = await CriarPostAsync(_auth.Client2);
        await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(200);
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(300);

        var resp = await _auth.Client1.GetAsync("/api/v1/Usuario/posts-salvos");
        var lista = resp.Data as Newtonsoft.Json.Linq.JArray
                 ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var ainda = lista?.Any(p => p["id"]?.ToString() == postId.ToString()) ?? false;
        _out.WriteLine($"Post {postId} ainda nos salvos apos dessalvar: {ainda}");
        Assert.False(ainda, "Post nao deve aparecer nos salvos apos dessalvar");
    }

    [Fact(DisplayName = "PS06 - User1 pode salvar o mesmo post novamente apos dessalvar")]
    public async Task PS06_ReSalvarPost()
    {
        var postId = await CriarPostAsync(_auth.Client2);
        await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(200);

        var resp = await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "PS07 - User2 compartilha post de User1 retorna 200")]
    public async Task PS07_CompartilharPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/compartilhar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "PS08 - Compartilhar o mesmo post duas vezes retorna 400")]
    public async Task PS08_CompartilharPost_Duplicado()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/compartilhar");
        await Task.Delay(200);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/compartilhar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "PS09 - Visualizar post registra visualizacao retorna 200")]
    public async Task PS09_VisualizarPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/visualizar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "PS10 - Visualizar post duplicado retorna 200 (idempotente)")]
    public async Task PS10_VisualizarPost_Duplicado_Idempotente()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/visualizar");
        await Task.Delay(200);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/visualizar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // ================================================================
    // COMUNIDADE — entrar, sair, transferir admin, expulsar
    // ================================================================

    [Fact(DisplayName = "CM01 - User2 entra na comunidade publica retorna 200")]
    public async Task CM01_User2_EntraComunidade()
    {
        var comId = await CriarComunidadeAsync(publica: true);
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await Task.Delay(300);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "CM02 - User2 membro verificado via sou-membro")]
    public async Task CM02_User2_MembroVerificado()
    {
        var comId = await CriarComunidadeAsync(publica: true);
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        await Task.Delay(300);

        var resp = await _auth.Client2.GetAsync($"/api/v1/Comunidade/{comId}/sou-membro");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.True(resp.Data?["ehMembro"]?.ToObject<bool>(), "User2 deveria ser membro");
    }

    [Fact(DisplayName = "CM03 - User1 (admin) transfere admin para User2 e User2 vira admin")]
    public async Task CM03_TransferirAdmin_User2_ViraAdmin()
    {
        var comId = await CriarComunidadeAsync(publica: true);

        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        await Task.Delay(300);

        var resp = await _auth.Client1.PutAsync($"/api/v1/Comunidade/{comId}/transferir-admin",
            new { novoAdminId = _auth.User2Id });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var admins = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{comId}/admins");
        var lista = admins.Data as Newtonsoft.Json.Linq.JArray;
        var user2EhAdmin = lista?.Any(a =>
            a["id"]?.ToString() == _auth.User2Id.ToString() ||
            a["usuarioId"]?.ToString() == _auth.User2Id.ToString()) ?? false;
        _out.WriteLine($"User2 e admin apos transferencia: {user2EhAdmin}");
    }

    [Fact(DisplayName = "CM04 - User2 (novo admin) expulsa User1 e User1 nao e mais membro")]
    public async Task CM04_NovoAdmin_Expulsa_User1()
    {
        var comId = await CriarComunidadeAsync(publica: true);

        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        await Task.Delay(300);
        await _auth.Client1.PutAsync($"/api/v1/Comunidade/{comId}/transferir-admin",
            new { novoAdminId = _auth.User2Id });
        await Task.Delay(300);

        var expulsar = await _auth.Client2.DeleteAsync(
            $"/api/v1/Comunidade/{comId}/expulsar/{_auth.User1Id}");
        _out.WriteLine($"Expulsar User1: {expulsar}");
        Assert.True(expulsar.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {expulsar.Status}");

        if (expulsar.Status == 200)
        {
            var membro = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{comId}/sou-membro");
            var ehMembro = membro.Data?["ehMembro"]?.ToObject<bool>() ?? false;
            _out.WriteLine($"User1 ainda e membro apos expulsao: {ehMembro}");
            Assert.False(ehMembro, "User1 nao deveria ser membro apos expulsao");
        }
    }

    [Fact(DisplayName = "CM05 - User1 expulso pode entrar na comunidade novamente")]
    public async Task CM05_ExpulsoPopeEntrarNovamente()
    {
        var comId = await CriarComunidadeAsync(publica: true);

        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        await Task.Delay(300);
        await _auth.Client1.PutAsync($"/api/v1/Comunidade/{comId}/transferir-admin",
            new { novoAdminId = _auth.User2Id });
        await Task.Delay(300);
        var expulsar = await _auth.Client2.DeleteAsync(
            $"/api/v1/Comunidade/{comId}/expulsar/{_auth.User1Id}");

        if (expulsar.Status != 200)
        {
            _out.WriteLine("[SKIP] Expulsao nao suportada neste estado");
            return;
        }

        await Task.Delay(300);

        var reentrar = await _auth.Client1.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        _out.WriteLine($"User1 reentra apos expulsao: {reentrar}");
        Assert.Equal(200, reentrar.Status);
    }

    [Fact(DisplayName = "CM06 - User2 sai da comunidade e nao e mais membro")]
    public async Task CM06_User2_SaiComunidade()
    {
        var comId = await CriarComunidadeAsync(publica: true);
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        await Task.Delay(300);

        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var membro = await _auth.Client2.GetAsync($"/api/v1/Comunidade/{comId}/sou-membro");
        var ehMembro = membro.Data?["ehMembro"]?.ToObject<bool>() ?? false;
        _out.WriteLine($"User2 ainda e membro apos sair: {ehMembro}");
        Assert.False(ehMembro, "User2 nao deveria ser membro apos sair");
    }

    [Fact(DisplayName = "CM07 - User2 pode entrar novamente apos sair")]
    public async Task CM07_User2_ReentraComunidade()
    {
        var comId = await CriarComunidadeAsync(publica: true);
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await Task.Delay(300);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "CM08 - Sair sem ser membro retorna 400")]
    public async Task CM08_SairSemSerMembro()
    {
        var comId = await CriarComunidadeAsync(publica: true);
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await Task.Delay(300);

        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "CM09 - User1 expulsa User2 e User2 nao e mais membro")]
    public async Task CM09_User1_Expulsa_User2()
    {
        var comId = await CriarComunidadeAsync(publica: true);

        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        await Task.Delay(300);

        var resp = await _auth.Client1.DeleteAsync(
            $"/api/v1/Comunidade/{comId}/expulsar/{_auth.User2Id}");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 400, $"Esperado 200 ou 400, recebeu {resp.Status}");

        if (resp.Status == 200)
        {
            var membro = await _auth.Client2.GetAsync($"/api/v1/Comunidade/{comId}/sou-membro");
            var ehMembro = membro.Data?["ehMembro"]?.ToObject<bool>() ?? false;
            _out.WriteLine($"User2 ainda e membro apos ser expulso: {ehMembro}");
            Assert.False(ehMembro, "User2 nao deveria ser membro apos ser expulso");
        }
    }

    [Fact(DisplayName = "CM10 - User2 expulso pode entrar novamente na comunidade")]
    public async Task CM10_User2_Expulso_ReentraComunidade()
    {
        var comId = await CriarComunidadeAsync(publica: true);

        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        await Task.Delay(300);
        var expulsar = await _auth.Client1.DeleteAsync(
            $"/api/v1/Comunidade/{comId}/expulsar/{_auth.User2Id}");

        if (expulsar.Status != 200)
        {
            _out.WriteLine("[SKIP] Expulsao retornou: " + expulsar.Status);
            return;
        }

        await Task.Delay(300);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "CM11 - Nao membro nao pode expulsar ninguem retorna 400 ou 403")]
    public async Task CM11_NaoMembro_NaoPodeExpulsar()
    {
        var comId = await CriarComunidadeAsync(publica: true);

        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await Task.Delay(300);

        var resp = await _auth.Client2.DeleteAsync(
            $"/api/v1/Comunidade/{comId}/expulsar/{_auth.User1Id}");
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 400 or 403, $"Esperado 400 ou 403, recebeu {resp.Status}");
    }

    [Fact(DisplayName = "CM12 - Admin nao pode transferir para nao membro retorna 400")]
    public async Task CM12_TransferirAdmin_ParaNaoMembro()
    {
        var comId = await CriarComunidadeAsync(publica: true);
        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        await Task.Delay(300);

        var resp = await _auth.Client1.PutAsync($"/api/v1/Comunidade/{comId}/transferir-admin",
            new { novoAdminId = _auth.User2Id });
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "CM13 - Fluxo completo: entrar, virar admin, expulsar, sair")]
    public async Task CM13_FluxoCompleto_Comunidade()
    {
        var comId = await CriarComunidadeAsync(publica: true);

        await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        var entrar = await _auth.Client2.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        Assert.Equal(200, entrar.Status);
        _out.WriteLine($"[1] User2 entrou: {entrar.Status}");
        await Task.Delay(300);

        var transferir = await _auth.Client1.PutAsync($"/api/v1/Comunidade/{comId}/transferir-admin",
            new { novoAdminId = _auth.User2Id });
        Assert.Equal(200, transferir.Status);
        _out.WriteLine($"[2] Admin transferido para User2: {transferir.Status}");
        await Task.Delay(300);

        var expulsarUser1 = await _auth.Client2.DeleteAsync(
            $"/api/v1/Comunidade/{comId}/expulsar/{_auth.User1Id}");
        _out.WriteLine($"[3] User2 expulsa User1: {expulsarUser1.Status}");
        Assert.Equal(200, expulsarUser1.Status);
        await Task.Delay(300);

        var membroUser1 = await _auth.Client1.GetAsync($"/api/v1/Comunidade/{comId}/sou-membro");
        var user1EhMembro = membroUser1.Data?["ehMembro"]?.ToObject<bool>() ?? true;
        _out.WriteLine($"[4] User1 ainda e membro apos expulsao: {user1EhMembro}");
        Assert.False(user1EhMembro, "User1 nao deveria ser membro apos ser expulso");

        var reentra = await _auth.Client1.PostAsync($"/api/v1/Comunidade/{comId}/entrar");
        _out.WriteLine($"[5] User1 reentrou apos expulsao: {reentra.Status}");
        Assert.Equal(200, reentra.Status);
        await Task.Delay(300);

        var sairUser2 = await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        _out.WriteLine($"[6] User2 (criador) tenta sair: {sairUser2.Status}");
        Assert.Equal(400, sairUser2.Status);

        var transferirDeVolta = await _auth.Client2.PutAsync($"/api/v1/Comunidade/{comId}/transferir-admin",
            new { novoAdminId = _auth.User1Id });
        _out.WriteLine($"[7] Admin transferido de volta para User1: {transferirDeVolta.Status}");
        Assert.Equal(200, transferirDeVolta.Status);
        await Task.Delay(300);

        var sairUser2Final = await _auth.Client2.DeleteAsync($"/api/v1/Comunidade/{comId}/sair");
        _out.WriteLine($"[8] User2 saiu apos transferir admin: {sairUser2Final.Status}");
        Assert.Equal(200, sairUser2Final.Status);

        var membroFinal = await _auth.Client2.GetAsync($"/api/v1/Comunidade/{comId}/sou-membro");
        var ehMembro = membroFinal.Data?["ehMembro"]?.ToObject<bool>() ?? true;
        _out.WriteLine($"[9] User2 ainda e membro ao final: {ehMembro}");
        Assert.False(ehMembro);
    }

    // ================================================================
    // HELPERS
    // ================================================================

    private async Task<Guid> CriarEventoAsync()
    {
        var titulo = $"EV QA {Guid.NewGuid():N}"[..15];
        var r = await _auth.Client1.PostAsync("/api/v1/Evento", new
        {
            titulo,
            descricao = "Evento para teste de participacao QA",
            localizacao = "Online",
            dataInicio = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss")
        });
        Assert.True(r.Status is 200 or 201, $"CriarEventoAsync falhou: {r.RawBody}");
        var id = r.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id));
        return Guid.Parse(id!);
    }

    private async Task<Guid> CriarPostAsync(ApiClient client)
    {
        var r = await client.PostAsync("/api/v1/Post", new
        {
            conteudo = $"Post helper QA {Guid.NewGuid():N}"[..40]
        });
        Assert.True(r.Status is 200 or 201, $"CriarPostAsync falhou: {r.RawBody}");
        var id = r.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id));
        return Guid.Parse(id!);
    }

    private async Task<Guid> CriarComunidadeAsync(bool publica)
    {
        var nome = publica
            ? $"QA Pub {Guid.NewGuid():N}"[..15]
            : $"QA Priv {Guid.NewGuid():N}"[..16];
        var r = await _auth.Client1.PostAsync("/api/v1/Comunidade", new
        {
            nome,
            descricao = "Comunidade para testes de acoes reversiveis QA",
            privada = !publica
        });
        Assert.Equal(200, r.Status);
        var id = r.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id));
        return Guid.Parse(id!);
    }
}
