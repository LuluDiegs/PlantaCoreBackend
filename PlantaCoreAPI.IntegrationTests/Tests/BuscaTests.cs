using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace PlantaCoreAPI.IntegrationTests.Tests;

[Collection("Integration")]
public class BuscaTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;

    public BuscaTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    [Fact(DisplayName = "B01 - Busca global com termo retorna 200")]
    public async Task B01_BuscaGlobal()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/busca?termo=planta");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "B02 - Busca global sem termo retorna 200")]
    public async Task B02_BuscaGlobal_SemTermo()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/busca?termo=");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "B03 - Busca de usuarios por nome retorna 200")]
    public async Task B03_BuscaUsuarios()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/busca/usuarios?nome=Luiza");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "B04 - Busca de usuarios sem nome retorna 200")]
    public async Task B04_BuscaUsuarios_SemNome()
    {
        var resp = await _auth.Client1.GetAsync("/api/v1/busca/usuarios?nome=");
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
    }

    [Fact(DisplayName = "B05 - Busca global retorna post por hashtag")]
    public async Task B05_BuscaGlobal_PorHashtag()
    {
        var termo = $"hashtagqa{Guid.NewGuid():N}";
        var postId = await CriarPostComMetadadosAsync(_auth.Client2, new
        {
            conteudo = $"Post de busca por hashtag #{termo}",
            hashtags = new[] { termo }
        });

        var resp = await _auth.Client1.GetAsync($"/api/v1/busca?termo={termo}");
        _out.WriteLine(resp.ToString());

        Assert.Equal(200, resp.Status);
        AssertPostEncontrado(resp.Json?["posts"], postId);
    }

    [Fact(DisplayName = "B06 - Busca global retorna post por categoria")]
    public async Task B06_BuscaGlobal_PorCategoria()
    {
        var planta = await ObterOuCriarPrimeiraPlantaAsync(_auth.Client2);

        var termo = planta.NomeComum ?? planta.NomeCientifico;

        var postId = await CriarPostComMetadadosAsync(_auth.Client2, new
        {
            conteudo = "Post de busca por categoria",
            plantaId = planta.Id
        });

        var resp = await _auth.Client1.GetAsync($"/api/v1/busca?termo={Uri.EscapeDataString(termo)}");
        _out.WriteLine(resp.ToString());

        Assert.Equal(200, resp.Status);
        AssertPostEncontrado(resp.Json?["posts"], postId);
    }

    [Fact(DisplayName = "B07 - Busca global retorna post por palavra-chave")]
    public async Task B07_BuscaGlobal_PorPalavraChave()
    {
        var termo = $"palavra-qa-{Guid.NewGuid():N}";
        var postId = await CriarPostComMetadadosAsync(_auth.Client2, new
        {
            conteudo = "Post de busca por palavra-chave",
            palavrasChave = new[] { termo }
        });

        var resp = await _auth.Client1.GetAsync($"/api/v1/busca?termo={termo}");
        _out.WriteLine(resp.ToString());

        Assert.Equal(200, resp.Status);
        AssertPostEncontrado(resp.Json?["posts"], postId);
    }

    private async Task<Guid> CriarPostComMetadadosAsync(ApiClient client, object body)
    {
        var resp = await client.PostAsync("/api/v1/Post", body);
        _out.WriteLine(resp.ToString());
        Assert.True(resp.Status is 200 or 201, $"Esperado 200 ou 201, recebeu {resp.Status}");

        var postId = resp.ExtractId();
        Assert.True(postId.HasValue, "A criação do post não retornou um id válido.");
        return postId.Value;
    }

    private static void AssertPostEncontrado(JToken? postsToken, Guid postId)
    {
        var posts = postsToken as JArray;
        Assert.NotNull(posts);
        Assert.Contains(posts!, p => string.Equals(p?["id"]?.ToString(), postId.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    private async Task<(Guid Id, string NomeCientifico, string NomeComum)> CriarPlantaViaIdentificacaoAsync(ApiClient client)
    {
        const string imageUrl = "https://bs.plantnet.org/image/o/a2b8cb049d071ed0bae3d324f7516b794399c4c8";

        using var http = new HttpClient();
        byte[] imageBytes = await http.GetByteArrayAsync(imageUrl);

        var form = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

        form.Add(fileContent, "Foto", "plant.jpg");
        form.Add(new StringContent("false"), "CriarPostagem");

        var resp = await client.PostMultipartAsync("/api/v1/Planta/identificar", form);
        _out.WriteLine(resp.ToString()); 
        Assert.Equal(200, resp.Status); 

        var planta = resp.Json?["planta"]; 
        Assert.NotNull(planta); 

        var id = planta["id"]?.ToString(); 
        var nomeCientifico = planta["nomeCientifico"]?.ToString(); 
        var nomeComum = planta["nomeComum"]?.ToString(); 

        return (Guid.Parse(id!), nomeCientifico!, nomeComum!);
    }

    private async Task<(Guid Id, string NomeCientifico, string? NomeComum)?> ObterPrimeiraPlantaAsync(ApiClient client)
    {
        var resp = await client.GetAsync("/api/v1/Planta/minhas-plantas?pagina=1&tamanho=1");
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

    private async Task<(Guid Id, string NomeCientifico, string? NomeComum)> ObterOuCriarPrimeiraPlantaAsync(ApiClient client)
    {
        var planta = await ObterPrimeiraPlantaAsync(client);

        if (planta is not null)
            return planta.Value;

        await CriarPlantaViaIdentificacaoAsync(client);

        planta = await ObterPrimeiraPlantaAsync(client);
        Assert.NotNull(planta);

        return planta.Value;
    }
}
