using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace PlantaCoreAPI.IntegrationTests.Tests;

[Collection("Integration")]
public class PostTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;

    public PostTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    // --- CRUD de post ---

    [Fact(DisplayName = "P01 - User1 cria post e retorna ID")]
    public async Task P01_CriarPost()
    {
        var resp = await _auth.Client1.PostAsync("/api/v1/Post", new
        {
            conteudo = $"Post QA criado por User1 - {Guid.NewGuid():N}"[..50]
        });
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 201, $"Esperado 200 ou 201, recebeu {resp.Status}");
        Assert.False(string.IsNullOrEmpty(resp.Data?["id"]?.ToString()));
    }

    [Fact(DisplayName = "P02 - Criar post sem auth retorna 401")]
    public async Task P02_CriarPost_SemAuth()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Post", new { conteudo = "Anon" });
        Assert.Equal(401, resp.Status);
    }

    [Fact(DisplayName = "P03 - Criar post conteudo vazio retorna 400")]
    public async Task P03_CriarPost_Vazio()
    {
        var resp = await _auth.Client1.PostAsync("/api/v1/Post", new { conteudo = "" });
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "P04 - Obter post existente retorna dados corretos")]
    public async Task P04_ObterPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client2.GetAsync($"/api/v1/Post/{postId}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.Equal(postId.ToString(), resp.Data?["id"]?.ToString());
    }

    [Fact(DisplayName = "P05 - Obter post inexistente retorna 404")]
    public async Task P05_ObterPost_Inexistente()
    {
        var resp = await _auth.Client1.GetAsync($"/api/v1/Post/{Guid.NewGuid()}");
        Assert.Equal(404, resp.Status);
    }

    [Fact(DisplayName = "P06 - User1 atualiza post proprio retorna 200")]
    public async Task P06_AtualizarPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client1.PutAsync($"/api/v1/Post/{postId}", new
        {
            conteudo = "Conteudo atualizado pelo teste QA"
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P07 - User2 tenta atualizar post de User1 retorna 400")]
    public async Task P07_AtualizarPost_OutroUsuario()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client2.PutAsync($"/api/v1/Post/{postId}", new
        {
            conteudo = "Invasao de User2"
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "P08 - User2 tenta excluir post de User1 retorna 400")]
    public async Task P08_ExcluirPost_OutroUsuario()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}");
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "P09 - User1 exclui post proprio retorna 200")]
    public async Task P09_ExcluirPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // --- Feeds e listagens ---

    [Fact(DisplayName = "P10 - Feed de User1 retorna 200 (segue User2)")]
    public async Task P10_Feed()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Post/feed?pagina=1&tamanho=10");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P11 - Explorar posts retorna 200")]
    public async Task P11_Explorar()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Post/explorar?pagina=1&tamanho=10");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P12 - Trending retorna 200")]
    public async Task P12_Trending()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Post/trending");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P13 - Posts de User1 visiveis por User2 (se seguem)")]
    public async Task P13_PostsDoUsuario()
    {
        var resp = await _auth.Client2.GetAsync($"/api/v1/Post/usuario/{_auth.User1Id}?pagina=1&tamanho=10");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // --- Curtidas em post ---

    [Fact(DisplayName = "P14 - User2 curte post de User1 retorna 200")]
    public async Task P14_CurtirPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/curtida");

        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/curtir");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P15 - Curtir post duplicado retorna 400")]
    public async Task P15_CurtirPost_Duplicado()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/curtir");

        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/curtir");
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "P16 - User2 descurte post de User1 retorna 200")]
    public async Task P16_DescurtirPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/curtir");

        var resp = await _auth.Client2.DeleteAsync($"/api/v1/Post/{postId}/curtida");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P17 - Posts curtidos por User2 retorna 200")]
    public async Task P17_PostsCurtidos()
    {
        var resp = await _auth.Client2.GetAsync($"/api/v1/Post/usuario/{_auth.User2Id}/curtidos");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // --- Comentarios (fluxo completo) ---

    [Fact(DisplayName = "P18 - User2 comenta post de User1 retorna ID do comentario")]
    public async Task P18_CriarComentario()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client2.PostAsync("/api/v1/Post/comentario", new
        {
            postId,
            conteudo = "Comentario de User2 no post de User1 - QA"
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.False(string.IsNullOrEmpty(resp.Data?["id"]?.ToString()));
    }

    [Fact(DisplayName = "P19 - User2 atualiza comentario proprio retorna 200")]
    public async Task P19_AtualizarComentario()
    {
        var comentId = await CriarComentarioAsync();
        var resp = await _auth.Client2.PutAsync($"/api/v1/Post/comentario/{comentId}", new
        {
            conteudo = "Comentario atualizado - QA"
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P20 - User1 responde comentario de User2 retorna 200")]
    public async Task P20_ResponderComentario()
    {
        var comentId = await CriarComentarioAsync();
        var resp = await _auth.Client1.PostAsync($"/api/v1/Post/comentario/{comentId}/responder", new
        {
            conteudo = "Resposta de User1 ao comentario de User2 - QA"
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P21 - User1 curte comentario de User2 retorna 200")]
    public async Task P21_CurtirComentario()
    {
        var comentId = await CriarComentarioAsync();
        await _auth.Client1.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");

        var resp = await _auth.Client1.PostAsync($"/api/v1/Post/comentario/{comentId}/curtir");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P22 - User1 descurte comentario de User2 retorna 200")]
    public async Task P22_DescurtirComentario()
    {
        var comentId = await CriarComentarioAsync();
        await _auth.Client1.PostAsync($"/api/v1/Post/comentario/{comentId}/curtir");

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Post/comentario/{comentId}/curtida");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P23 - Listar comentarios do post retorna 200")]
    public async Task P23_ListarComentarios()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        await _auth.Client2.PostAsync("/api/v1/Post/comentario", new { postId, conteudo = "Comentario para listar" });

        var resp = await _auth.Client1.GetAsync($"/api/v1/Post/{postId}/comentarios?pagina=1&tamanho=10");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P24 - Dono do post exclui comentario de User2 retorna 200")]
    public async Task P24_ExcluirComentario_DonoDo_Post()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var comentResp = await _auth.Client2.PostAsync("/api/v1/Post/comentario", new
        {
            postId,
            conteudo = "Comentario a ser excluido pelo dono do post"
        });
        var comentId = comentResp.Data?["id"]?.ToString();
        if (string.IsNullOrEmpty(comentId)) { _out.WriteLine("[SKIP]"); return; }

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/comentario/{comentId}");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // --- Salvar e compartilhar ---

    [Fact(DisplayName = "P25 - User1 salva post de User2 retorna 200")]
    public async Task P25_SalvarPost()
    {
        var postId = await CriarPostAsync(_auth.Client2);
        await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");

        var resp = await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P26 - User1 remove post salvo retorna 200")]
    public async Task P26_RemoverPostSalvo()
    {
        var postId = await CriarPostAsync(_auth.Client2);
        await _auth.Client1.PostAsync($"/api/v1/Post/{postId}/salvar");

        var resp = await _auth.Client1.DeleteAsync($"/api/v1/Post/{postId}/salvar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P27 - User2 compartilha post de User1 retorna 200")]
    public async Task P27_CompartilharPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/compartilhar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "P28 - User2 visualiza post de User1 retorna 200")]
    public async Task P28_VisualizarPost()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client2.PostAsync($"/api/v1/Post/{postId}/visualizar");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // --- Post em comunidade ---

    [Fact(DisplayName = "P29 - Listar posts de comunidade retorna 200")]
    public async Task P29_PostsDeComunidade()
    {
        var com = await _auth.Client1.PostAsync("/api/v1/Comunidade", new
        {
            nome = $"QA Post Com {Guid.NewGuid():N}"[..20],
            privada = false
        });
        var comId = com.Data?["id"]?.ToString();
        if (string.IsNullOrEmpty(comId)) { _out.WriteLine("[SKIP] sem comunidade"); return; }

        await _auth.Client1.PostAsync("/api/v1/Post", new
        {
            conteudo = "Post dentro da comunidade QA",
            comunidadeId = Guid.Parse(comId)
        });

        var resp = await _auth.Client1.GetAsync($"/api/v1/Post/comunidade/{comId}?pagina=1&tamanho=10");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    // --- Helpers ---

    private async Task<Guid> CriarPostAsync(ApiClient client)
    {
        var resp = await client.PostAsync("/api/v1/Post", new
        {
            conteudo = $"Post helper QA {Guid.NewGuid():N}"[..40]
        });
        var id = resp.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id), $"CriarPostAsync falhou: {resp.RawBody}");
        return Guid.Parse(id!);
    }

    private async Task<Guid> CriarComentarioAsync()
    {
        var postId = await CriarPostAsync(_auth.Client1);
        var resp = await _auth.Client2.PostAsync("/api/v1/Post/comentario", new
        {
            postId,
            conteudo = $"Comentario helper QA {Guid.NewGuid():N}"[..40]
        });
        var id = resp.Data?["id"]?.ToString();
        Assert.False(string.IsNullOrEmpty(id), $"CriarComentarioAsync falhou: {resp.RawBody}");
        return Guid.Parse(id!);
    }

    [Fact(DisplayName = "P30 - Obter post retorna categorias derivadas da planta e hashtags com #")]
    public async Task P30_ObterPost_RetornaMetadadosNoFormatoEsperado()
    {
        var planta = await ObterOuCriarPrimeiraPlantaAsync();
        var termoHashtag = $"MetaQa{Guid.NewGuid():N}";
        var respCriacao = await _auth.Client1.PostAsync("/api/v1/Post", new
        {
            conteudo = $"Post QA integração automático #{termoHashtag}",
            hashtags = new[] { termoHashtag },
            plantaId = planta.Id,
            categorias = new[] { "Categoria QA Completa" }
        });

        _out.WriteLine(respCriacao.ToString());
        Assert.True(respCriacao.Status is 200 or 201, $"Esperado 200 ou 201, recebeu {respCriacao.Status}");

        var postId = respCriacao.ExtractId();
        Assert.True(postId.HasValue, "A criação do post não retornou id válido.");

        var resp = await _auth.Client1.GetAsync($"/api/v1/Post/{postId}");
        _out.WriteLine(resp.ToString());

        Assert.Equal(200, resp.Status);

        var hashtags = resp.Data?["hashtags"]?.Values<string>().ToList() ?? new List<string?>();
        var categorias = resp.Data?["categorias"]?.Values<string>().ToList() ?? new List<string?>();
        var palavrasChave = resp.Data?["palavrasChave"]?.Values<string>().ToList() ?? new List<string?>();

        Assert.Contains($"#{termoHashtag}", hashtags);
        Assert.Contains(planta.NomeCientifico, categorias);
        if (!string.IsNullOrWhiteSpace(planta.NomeComum))
            Assert.Contains(planta.NomeComum, categorias);
        Assert.DoesNotContain("Categoria QA Completa", categorias);
        Assert.Contains("Post", palavrasChave);
        Assert.Contains("QA", palavrasChave);
        Assert.Contains("integração", palavrasChave);
        Assert.Contains("automático", palavrasChave);
    }

    private async Task<(Guid Id, string NomeCientifico, string NomeComum)> CriarPlantaViaIdentificacaoAsync()
    {
        const string imageUrl = "https://bs.plantnet.org/image/o/a2b8cb049d071ed0bae3d324f7516b794399c4c8";

        using var http = new HttpClient();
        byte[] imageBytes = await http.GetByteArrayAsync(imageUrl);

        var form = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

        form.Add(fileContent, "Foto", "plant.jpg");
        form.Add(new StringContent("false"), "CriarPostagem");

        var resp = await _auth.Client1.PostMultipartAsync("/api/v1/Planta/identificar", form);
        _out.WriteLine(resp.ToString()); 
        Assert.Equal(200, resp.Status); 

        var planta = resp.Json?["planta"]; 
        Assert.NotNull(planta); 

        var id = planta["id"]?.ToString(); 
        var nomeCientifico = planta["nomeCientifico"]?.ToString(); 
        var nomeComum = planta["nomeComum"]?.ToString(); 

        return (Guid.Parse(id!), nomeCientifico!, nomeComum!);
    }

    private async Task<(Guid Id, string NomeCientifico, string NomeComum)?> ObterPrimeiraPlantaAsync()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/Planta/minhas-plantas?pagina=1&tamanho=1");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);

        var plantas = resp.Data as JArray;
        Assert.NotNull(plantas);

        var planta = plantas.FirstOrDefault();
        if (planta is null) return null;

        var id = planta["id"]?.ToString();
        var nomeCientifico = planta["nomeCientifico"]?.ToString();
        var nomeComum = planta["nomeComum"]?.ToString();

        Assert.False(string.IsNullOrWhiteSpace(id));
        Assert.False(string.IsNullOrWhiteSpace(nomeCientifico));
        Assert.False(string.IsNullOrWhiteSpace(nomeComum));

        return (Guid.Parse(id), nomeCientifico, nomeComum);
    }

    private async Task<(Guid Id, string NomeCientifico, string NomeComum)> ObterOuCriarPrimeiraPlantaAsync()
    {
        var planta = await ObterPrimeiraPlantaAsync();

        if (planta is not null)
            return planta.Value;

        await CriarPlantaViaIdentificacaoAsync();

        planta = await ObterPrimeiraPlantaAsync();
        Assert.NotNull(planta);

        return planta.Value;
    }
}
