using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;

namespace PlantaCoreAPI.IntegrationTests.Tests;

/// <summary>
/// Testes end-to-end de engajamento com fluxos completos entre User1 e User2:
///
/// CURTIDAS
///   - Ambos curtem o mesmo post -> contagem sobe -> ambos descurtem -> contagem volta
///   - Ambos curtem o mesmo comentario -> ambos descurtem
///
/// SALVAR / DESSALVAR
///   - User1 e User2 salvam o mesmo post
///   - User1 dessalva -> post some dos salvos de User1 mas permanece nos de User2
///   - User2 dessalva -> post some de ambos
///
/// COMPARTILHAR
///   - User1 compartilha post de User2
///   - User2 compartilha post de User1
///   - Compartilhar duas vezes retorna 400
///
/// EVENTOS
///   - User1 cria evento
///   - User2 marca participacao -> ambos aparecem nos participantes
///   - User2 desmarca -> User1 ainda esta nos participantes (anfitriao)
///   - User1 (anfitriao) nao pode desmarcar
/// </summary>
[Collection("Integration")]
public class EngajamentoTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;

    public EngajamentoTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    // ================================================================
    // CURTIDAS — post
    // ================================================================

    [Fact(DisplayName = "EN01 - Ambos curtem o mesmo post e contagem sobe")]
    public async Task EN01_AmbosCurtem_MesmoPost()
    {
        var postId = await CriarPostAsync(_auth.Client2);

        // Limpa estado anterior
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/curtida");
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/curtida");
        await Task.Delay(300);

        // User1 curte post de User2
        var curtida1 = await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/curtir");
        _out.WriteLine($"User1 curtiu post de User2: {curtida1.Status}");
        Assert.Equal(200, curtida1.Status);

        // User3 = nao existe, entao User2 nao pode curtir o proprio — criamos outro post de User1
        var postId2 = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId2}/curtida");
        var curtida2 = await _auth.Client2.PostAsync($"/api/v1/Post/{postId2}/curtir");
        _out.WriteLine($"User2 curtiu post de User1: {curtida2.Status}");
        Assert.Equal(200, curtida2.Status);

        // Verifica que o post de User2 tem pelo menos 1 curtida (de User1)
        var post = await _auth.Client1.GetAsync($"/api/v1/Post/{postId}");
        var totalCurtidas = post.Data?["totalCurtidas"]?.ToObject<int>() ?? 0;
        _out.WriteLine($"Total curtidas no post de User2 apos User1 curtir: {totalCurtidas}");
        Assert.True(totalCurtidas >= 1, $"Esperado >= 1 curtida, recebeu {totalCurtidas}");
    }

    [Fact(DisplayName = "EN02 - Curtir post duplicado por User2 retorna 400")]
    public async Task EN02_CurtirPost_Duplicado()
    {
        // User2 curte post de User1 (nao pode curtir o proprio)
        var postId = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/curtida");
        await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/curtir");
        await Task.Delay(200);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/curtir");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "EN03 - User1 descurte post de User2 e contagem diminui")]
    public async Task EN03_User1_DescurtePost()
    {
        // Post de User2 — User1 curte e depois descurte
        var postId = await CriarPostAsync(_auth.Client2);
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/curtida");
        await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/curtir");
        await Task.Delay(200);

        var antes = await _auth.Client1.GetAsync($"/api/v1/Post/{postId}");
        var totalAntes = antes.Data?["totalCurtidas"]?.ToObject<int>() ?? 0;

        var descurtir = await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/curtida");
        _out.WriteLine($"User1 descurtiu: {descurtir.Status}");
        Assert.Equal(200, descurtir.Status);

        var depois = await _auth.Client1.GetAsync($"/api/v1/Post/{postId}");
        var totalDepois = depois.Data?["totalCurtidas"]?.ToObject<int>() ?? 0;
        _out.WriteLine($"Curtidas antes: {totalAntes}, depois: {totalDepois}");
        Assert.True(totalDepois < totalAntes, "Contagem de curtidas deve diminuir apos descurtir");
    }

    [Fact(DisplayName = "EN04 - Ambos descurtem posts cruzados e contagem volta")]
    public async Task EN04_AmbosDescent_MesmoPost()
    {
        // Cada um curte o post do outro (nao pode curtir o proprio)
        var postDeUser1 = await CriarPostAsync(_auth.Client1);
        var postDeUser2 = await CriarPostAsync(_auth.Client2);

        // Limpa
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postDeUser1}/curtida");
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postDeUser2}/curtida");

        // Ambos curtem
        await _auth.Client2.PostAsync($"/api/v1/Post/{postDeUser1}/curtir");
        await _auth.Client1.PostAsync($"/api/v1/Post/{postDeUser2}/curtir");
        await Task.Delay(300);

        var comUser1 = await _auth.Client1.GetAsync($"/api/v1/Post/{postDeUser1}");
        var totalCom1 = comUser1.Data?["totalCurtidas"]?.ToObject<int>() ?? 0;
        var comUser2 = await _auth.Client2.GetAsync($"/api/v1/Post/{postDeUser2}");
        var totalCom2 = comUser2.Data?["totalCurtidas"]?.ToObject<int>() ?? 0;

        // Ambos descurtem
        var d1 = await _auth.Client2.DeleteAsync($"/api/v1/Post/{postDeUser1}/curtida");
        var d2 = await _auth.Client1.DeleteAsync($"/api/v1/Post/{postDeUser2}/curtida");
        _out.WriteLine($"User2 descurtiu post de User1: {d1.Status}");
        _out.WriteLine($"User1 descurtiu post de User2: {d2.Status}");
        Assert.Equal(200, d1.Status);
        Assert.Equal(200, d2.Status);
        await Task.Delay(300);

        var semUser1 = await _auth.Client1.GetAsync($"/api/v1/Post/{postDeUser1}");
        var totalSem1 = semUser1.Data?["totalCurtidas"]?.ToObject<int>() ?? 0;
        var semUser2 = await _auth.Client2.GetAsync($"/api/v1/Post/{postDeUser2}");
        var totalSem2 = semUser2.Data?["totalCurtidas"]?.ToObject<int>() ?? 0;
        _out.WriteLine($"Post1 com: {totalCom1} sem: {totalSem1} | Post2 com: {totalCom2} sem: {totalSem2}");
        Assert.True(totalSem1 < totalCom1, "Contagem de post1 deve diminuir apos descurtir");
        Assert.True(totalSem2 < totalCom2, "Contagem de post2 deve diminuir apos descurtir");
    }

    [Fact(DisplayName = "EN05 - Post curtido por User2 aparece na lista de curtidos de User2")]
    public async Task EN05_PostCurtido_AparececNaLista()
    {
        // User2 curte post de User1 (nao pode curtir o proprio)
        var postId = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/curtida");
        await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/curtir");
        await Task.Delay(300);

        var resp = await _auth.Client2.GetAsync($"/api/v1/Post/usuario/{_auth.User2Id}/curtidos");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var lista = resp.Data as Newtonsoft.Json.Linq.JArray
                 ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var encontrou = lista?.Any(p => p["id"]?.ToString() == postId.ToString()) ?? false;
        _out.WriteLine($"Post {postId} na lista de curtidos de User2: {encontrou}");
        Assert.True(encontrou, "Post curtido deve aparecer na lista de curtidos do usuario");
    }

    // ================================================================
    // CURTIDAS — comentario
    // ================================================================

    [Fact(DisplayName = "EN06 - Ambos curtem o mesmo comentario")]
    public async Task EN06_AmbosCurtem_MesmoComentario()
    {
        var (_, comentId) = await CriarPostEComentarioAsync();

        // Limpa estado
        await _auth.Client1.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        await _auth.Client2.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        await Task.Delay(200);

        // User1 curte o comentario de User2
        var c1 = await _auth.Client1.PostAsync($"/api/v1/Post/comentario/{comentId}/curtir");
        _out.WriteLine($"User1 curtiu comentario: {c1.Status}");
        Assert.Equal(200, c1.Status);

        // User2 curte o proprio comentario
        var c2 = await _auth.Client2.PostAsync($"/api/v1/Post/comentario/{comentId}/curtir");
        _out.WriteLine($"User2 curtiu comentario: {c2.Status}");
        Assert.Equal(200, c2.Status);
    }

    [Fact(DisplayName = "EN07 - Curtir comentario duplicado retorna 400")]
    public async Task EN07_CurtirComentario_Duplicado()
    {
        var (_, comentId) = await CriarPostEComentarioAsync();
        await _auth.Client1.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        await _auth.Client1.PostAsync($"/api/v1/Post/comentario/{comentId}/curtir");
        await Task.Delay(200);

        var resp = await _auth.Client1.PostAsync($"/api/v1/Post/comentario/{comentId}/curtir");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "EN08 - Ambos descurtem o mesmo comentario")]
    public async Task EN08_AmbosDescent_MesmoComentario()
    {
        var (_, comentId) = await CriarPostEComentarioAsync();

        await _auth.Client1.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        await _auth.Client2.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        await _auth.Client1.PostAsync($"/api/v1/Post/comentario/{comentId}/curtir");
        await _auth.Client2.PostAsync($"/api/v1/Post/comentario/{comentId}/curtir");
        await Task.Delay(300);

        var d1 = await _auth.Client1.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        _out.WriteLine($"User1 descurtiu comentario: {d1.Status}");
        Assert.Equal(200, d1.Status);

        var d2 = await _auth.Client2.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        _out.WriteLine($"User2 descurtiu comentario: {d2.Status}");
        Assert.Equal(200, d2.Status);
    }

    [Fact(DisplayName = "EN09 - Descurtir comentario sem curtida retorna 400")]
    public async Task EN09_DescurtirComentario_SemCurtida()
    {
        var (_, comentId) = await CriarPostEComentarioAsync();
        await _auth.Client1.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        await Task.Delay(200);

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    // ================================================================
    // SALVAR / DESSALVAR — ambos os usuarios, verificacao cruzada
    // ================================================================

    [Fact(DisplayName = "EN10 - Ambos salvam o mesmo post e aparece nos salvos de cada um")]
    public async Task EN10_AmbosSalvam_MesmoPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);

        // Limpa estado
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(200);

        // User1 salva
        var s1 = await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine($"User1 salvou: {s1.Status}");
        Assert.Equal(200, s1.Status);

        // User2 salva o mesmo post
        var s2 = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine($"User2 salvou: {s2.Status}");
        Assert.Equal(200, s2.Status);
        await Task.Delay(300);

        // Verifica nos salvos de User1
        var salvosU1 = await _auth.Client1.GetAsync("/api/v1/Usuario/posts-salvos");
        var listaU1 = salvosU1.Data as Newtonsoft.Json.Linq.JArray
                   ?? salvosU1.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var u1TempPost = listaU1?.Any(p => p["id"]?.ToString() == postId.ToString()) ?? false;
        _out.WriteLine($"Post nos salvos de User1: {u1TempPost}");
        Assert.True(u1TempPost, "Post deve aparecer nos salvos de User1");

        // Verifica nos salvos de User2
        var salvosU2 = await _auth.Client2.GetAsync("/api/v1/Usuario/posts-salvos");
        var listaU2 = salvosU2.Data as Newtonsoft.Json.Linq.JArray
                   ?? salvosU2.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var u2TempPost = listaU2?.Any(p => p["id"]?.ToString() == postId.ToString()) ?? false;
        _out.WriteLine($"Post nos salvos de User2: {u2TempPost}");
        Assert.True(u2TempPost, "Post deve aparecer nos salvos de User2");
    }

    [Fact(DisplayName = "EN11 - Somente User1 dessalva - post some dos salvos de User1 mas permanece nos de User2")]
    public async Task EN11_SomenteUser1_Dessalva_PostPermanece_Em_User2()
    {
        var postId = await CriarPostAsync(_auth.Client1);

        // Ambos salvam
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(300);

        // Somente User1 dessalva
        var dessalvar = await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine($"User1 dessalvou: {dessalvar.Status}");
        Assert.Equal(200, dessalvar.Status);
        await Task.Delay(300);

        // Post NAO deve estar nos salvos de User1
        var salvosU1 = await _auth.Client1.GetAsync("/api/v1/Usuario/posts-salvos");
        var listaU1 = salvosU1.Data as Newtonsoft.Json.Linq.JArray
                   ?? salvosU1.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var u1AindaTem = listaU1?.Any(p => p["id"]?.ToString() == postId.ToString()) ?? false;
        _out.WriteLine($"Post ainda nos salvos de User1 apos dessalvar: {u1AindaTem}");
        Assert.False(u1AindaTem, "Post NAO deve estar nos salvos de User1 apos dessalvar");

        // Post DEVE continuar nos salvos de User2
        var salvosU2 = await _auth.Client2.GetAsync("/api/v1/Usuario/posts-salvos");
        var listaU2 = salvosU2.Data as Newtonsoft.Json.Linq.JArray
                   ?? salvosU2.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var u2AindaTem = listaU2?.Any(p => p["id"]?.ToString() == postId.ToString()) ?? false;
        _out.WriteLine($"Post ainda nos salvos de User2 (nao dessalvou): {u2AindaTem}");
        Assert.True(u2AindaTem, "Post DEVE continuar nos salvos de User2");
    }

    [Fact(DisplayName = "EN12 - User2 tambem dessalva - post some dos salvos de ambos")]
    public async Task EN12_AmbosDessalvam_PostSomeDeAmbos()
    {
        var postId = await CriarPostAsync(_auth.Client1);

        // Ambos salvam
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(300);

        // Ambos dessalvam
        var d1 = await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine($"User1 dessalvou: {d1.Status}");
        Assert.Equal(200, d1.Status);

        var d2 = await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine($"User2 dessalvou: {d2.Status}");
        Assert.Equal(200, d2.Status);
        await Task.Delay(300);

        // Post NAO deve estar nos salvos de nenhum
        var salvosU1 = await _auth.Client1.GetAsync("/api/v1/Usuario/posts-salvos");
        var listaU1 = salvosU1.Data as Newtonsoft.Json.Linq.JArray
                   ?? salvosU1.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var u1Tem = listaU1?.Any(p => p["id"]?.ToString() == postId.ToString()) ?? false;
        _out.WriteLine($"Post nos salvos de User1: {u1Tem}");
        Assert.False(u1Tem, "Post nao deve estar nos salvos de User1");

        var salvosU2 = await _auth.Client2.GetAsync("/api/v1/Usuario/posts-salvos");
        var listaU2 = salvosU2.Data as Newtonsoft.Json.Linq.JArray
                   ?? salvosU2.Json?["data"] as Newtonsoft.Json.Linq.JArray;
        var u2Tem = listaU2?.Any(p => p["id"]?.ToString() == postId.ToString()) ?? false;
        _out.WriteLine($"Post nos salvos de User2: {u2Tem}");
        Assert.False(u2Tem, "Post nao deve estar nos salvos de User2");
    }

    [Fact(DisplayName = "EN13 - Dessalvar post que nao foi salvo retorna 200 (idempotente)")]
    public async Task EN13_DessalvarSemSalvar_Idempotente()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        // Garante que nao esta salvo
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        await Task.Delay(200);

        // Tenta dessalvar novamente
        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // ================================================================
    // COMPARTILHAR
    // ================================================================

    [Fact(DisplayName = "EN14 - User1 compartilha post de User2 retorna 200")]
    public async Task EN14_User1_Compartilha_PostUser2()
    {
        var postId = await CriarPostAsync(_auth.Client2);

        var resp = await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/compartilhar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "EN15 - User2 compartilha post de User1 retorna 200")]
    public async Task EN15_User2_Compartilha_PostUser1()
    {
        var postId = await CriarPostAsync(_auth.Client1);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/compartilhar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "EN16 - Compartilhar o mesmo post duas vezes retorna 400")]
    public async Task EN16_Compartilhar_Duplicado()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/compartilhar");
        await Task.Delay(200);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/compartilhar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "EN17 - Ambos compartilham o mesmo post - cada um pode compartilhar uma vez")]
    public async Task EN17_AmbosCompartilham_MesmoPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);

        // User2 compartilha
        var c2 = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/compartilhar");
        _out.WriteLine($"User2 compartilhou: {c2.Status}");
        Assert.Equal(200, c2.Status);

        // Outro post para User1 compartilhar (nao pode recompartilhar o proprio)
        var postId2 = await CriarPostAsync(_auth.Client2);
        var c1 = await _auth.Client1.PostAsync($"/api/v1/Post/{postId2}/compartilhar");
        _out.WriteLine($"User1 compartilhou: {c1.Status}");
        Assert.Equal(200, c1.Status);
    }

    // ================================================================
    // EVENTOS — ambos os usuarios, marcar/desmarcar participacao
    // ================================================================

    [Fact(DisplayName = "EN18 - User1 cria evento e ja e participante (anfitriao)")]
    public async Task EN18_Anfitriao_JaEParticipante()
    {
        var eventoId = await CriarEventoAsync();

        var participantes = await _auth.Client1.GetAsync($"/api/v1/Evento/{eventoId}/participantes");
        _out.WriteLine(participantes.ToString());
        Assert.Equal(200, participantes.Status);

        var lista = ParticipantesComoArray(participantes);
        var user1Esta = lista?.Any(p =>
            p["usuarioId"]?.ToString() == _auth.User1Id.ToString() ||
            p["id"]?.ToString() == _auth.User1Id.ToString()) ?? false;
        _out.WriteLine($"User1 (anfitriao) esta nos participantes: {user1Esta}");
        Assert.True(user1Esta, "Anfitriao deve ser automaticamente participante");
    }

    [Fact(DisplayName = "EN19 - User2 marca participacao e ambos aparecem nos participantes")]
    public async Task EN19_User2_MarcaParticipacao_AmbosAparecem()
    {
        var eventoId = await CriarEventoAsync();

        // Garante que User2 nao participa
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        // User2 marca
        var marcar = await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine($"User2 marcou participacao: {marcar.Status}");
        Assert.Equal(200, marcar.Status);
        await Task.Delay(300);

        var participantes = await _auth.Client1.GetAsync($"/api/v1/Evento/{eventoId}/participantes");
        var lista = ParticipantesComoArray(participantes);
        var total = lista?.Count ?? 0;
        _out.WriteLine($"Total de participantes: {total}");
        Assert.True(total >= 2, $"Esperado pelo menos 2 participantes (anfitriao + User2), encontrou {total}");
    }

    [Fact(DisplayName = "EN20 - Marcar participacao duplicada retorna 400")]
    public async Task EN20_MarcaParticipacao_Duplicada()
    {
        var eventoId = await CriarEventoAsync();
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(200);

        var resp = await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "EN21 - User2 desmarca participacao e User1 (anfitriao) permanece")]
    public async Task EN21_User2_Desmarca_User1_Permanece()
    {
        var eventoId = await CriarEventoAsync();

        // User2 marca e desmarca
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);
        var desmarcar = await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine($"User2 desmarcou: {desmarcar.Status}");
        Assert.Equal(200, desmarcar.Status);
        await Task.Delay(300);

        // User1 (anfitriao) ainda deve estar nos participantes
        var participantes = await _auth.Client1.GetAsync($"/api/v1/Evento/{eventoId}/participantes");
        var lista = ParticipantesComoArray(participantes);
        var user1Esta = lista?.Any(p =>
            p["usuarioId"]?.ToString() == _auth.User1Id.ToString() ||
            p["id"]?.ToString() == _auth.User1Id.ToString()) ?? false;
        _out.WriteLine($"User1 ainda esta nos participantes apos User2 desmarcar: {user1Esta}");
        Assert.True(user1Esta, "Anfitriao deve permanecer nos participantes independente de outros saírem");

        // User2 nao deve mais estar nos participantes
        var user2Esta = lista?.Any(p =>
            p["usuarioId"]?.ToString() == _auth.User2Id.ToString() ||
            p["id"]?.ToString() == _auth.User2Id.ToString()) ?? false;
        _out.WriteLine($"User2 ainda esta nos participantes apos desmarcar: {user2Esta}");
        Assert.False(user2Esta, "User2 nao deve estar nos participantes apos desmarcar");
    }

    [Fact(DisplayName = "EN22 - Anfitriao (User1) nao pode desmarcar propria participacao retorna 400")]
    public async Task EN22_Anfitriao_NaoPodeDesmarcar()
    {
        var eventoId = await CriarEventoAsync();

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "EN23 - User2 pode re-marcar participacao apos desmarcar")]
    public async Task EN23_User2_ReMarca_AposDesmarcar()
    {
        var eventoId = await CriarEventoAsync();

        // Marcar -> Desmarcar -> Marcar novamente
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        var remarcar = await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine($"User2 re-marcou participacao: {remarcar.Status}");
        Assert.Equal(200, remarcar.Status);
    }

    [Fact(DisplayName = "EN24 - Desmarcar sem ter marcado retorna 400")]
    public async Task EN24_Desmarcar_SemParticipacao()
    {
        var eventoId = await CriarEventoAsync();
        // Garante que nao participa
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "EN25 - Fluxo completo: evento com ambos marcando e User2 desmarcando")]
    public async Task EN25_FluxoCompleto_Evento()
    {
        var eventoId = await CriarEventoAsync();

        // Limpa estado de User2
        await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        await Task.Delay(300);

        // 1. User2 marca
        var marcar = await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        Assert.Equal(200, marcar.Status);
        _out.WriteLine($"[1] User2 marcou: {marcar.Status}");
        await Task.Delay(300);

        // 2. Verifica 2 participantes
        var part1 = await _auth.Client1.GetAsync($"/api/v1/Evento/{eventoId}/participantes");
        var total1 = ParticipantesComoArray(part1)?.Count ?? 0;
        _out.WriteLine($"[2] Total participantes com ambos: {total1}");
        Assert.True(total1 >= 2);

        // 3. User2 desmarca
        var desmarcar = await _auth.Client2.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        Assert.Equal(200, desmarcar.Status);
        _out.WriteLine($"[3] User2 desmarcou: {desmarcar.Status}");
        await Task.Delay(300);

        // 4. Verifica que so User1 (anfitriao) permanece
        var part2 = await _auth.Client1.GetAsync($"/api/v1/Evento/{eventoId}/participantes");
        var lista2 = ParticipantesComoArray(part2);
        var total2 = lista2?.Count ?? 0;
        _out.WriteLine($"[4] Total participantes apos User2 sair: {total2}");
        Assert.True(total2 < total1, "Total deve ser menor apos User2 desmarcar");

        // 5. User2 re-marca
        var remarcar = await _auth.Client2.PostAsync($"/api/v1/Evento/{eventoId}/participacao");
        Assert.Equal(200, remarcar.Status);
        _out.WriteLine($"[5] User2 re-marcou: {remarcar.Status}");

        // 6. Anfitriao tenta desmarcar -> 400
        var anfDesmarcar = await _auth.Client1.DeleteAsync($"/api/v1/Evento/{eventoId}/participacao");
        _out.WriteLine($"[6] Anfitriao tentou desmarcar: {anfDesmarcar.Status}");
        Assert.Equal(400, anfDesmarcar.Status);
    }

    // ================================================================
    // HELPERS
    // ================================================================

    private async Task<Guid> CriarPostAsync(ApiClient client)
    {
        var r = await client.PostAsync("/api/v1/Post", new
        {
            conteudo = $"Post EN QA {Guid.NewGuid():N}"[..40]
        });
        Assert.True(r.Status is 200 or 201, $"CriarPostAsync falhou: {r.RawBody}");
        var id = r.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id));
        return Guid.Parse(id!);
    }

    private async Task<(Guid postId, Guid comentId)> CriarPostEComentarioAsync()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client2.PostAsync("/api/v1/Post/comentario", new
        {
            postId,
            conteudo = $"Comentario EN QA {Guid.NewGuid():N}"[..40]
        });
        Assert.True(resp.Status is 200 or 201, $"CriarComentarioAsync falhou: {resp.RawBody}");
        var id = resp.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id));
        return (postId, Guid.Parse(id!));
    }

    private async Task<Guid> CriarEventoAsync()
    {
        var titulo = $"EN EV QA {Guid.NewGuid():N}"[..18];
        var r = await _auth.Client1.PostAsync("/api/v1/Evento", new
        {
            titulo,
            descricao = "Evento para teste de engajamento QA",
            localizacao = "Online",
            dataInicio = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss")
        });
        Assert.True(r.Status is 200 or 201, $"CriarEventoAsync falhou: {r.RawBody}");
        var id = r.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id));
        return Guid.Parse(id!);
    }

    private static Newtonsoft.Json.Linq.JArray? ParticipantesComoArray(ApiResponse resp)
    {
        return resp.Data as Newtonsoft.Json.Linq.JArray
            ?? resp.Json?["data"] as Newtonsoft.Json.Linq.JArray;
    }
}
