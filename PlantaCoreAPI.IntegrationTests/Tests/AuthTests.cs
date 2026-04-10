using Xunit;
using Xunit.Abstractions;
using PlantaCoreAPI.IntegrationTests.Infrastructure;

namespace PlantaCoreAPI.IntegrationTests.Tests;

[Collection("Integration")]
public class AuthTests
{
    private readonly SharedAuthFixture _auth;
    private readonly ITestOutputHelper _out;

    public AuthTests(SharedAuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _out = output;
    }

    [Fact(DisplayName = "A01 - Login valido retorna token")]
    public async Task A01_Login_Valido()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Autenticacao/login", new
        {
            email = TestContext.User1Email,
            senha = TestContext.User1Senha
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.False(string.IsNullOrEmpty(resp.Data?["tokenAcesso"]?.ToString()));
    }

    [Fact(DisplayName = "A02 - Login senha errada retorna 401")]
    public async Task A02_Login_SenhaErrada()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Autenticacao/login", new
        {
            email = TestContext.User1Email,
            senha = "SenhaErrada999@"
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(401, resp.Status);
    }

    [Fact(DisplayName = "A03 - Login email inexistente retorna 401")]
    public async Task A03_Login_EmailInexistente()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Autenticacao/login", new
        {
            email = "naoexiste_qa_xyz@test123.com",
            senha = "Qualquer1@"
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(401, resp.Status);
    }

    [Fact(DisplayName = "A04 - Login email vazio retorna 400")]
    public async Task A04_Login_EmailVazio()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Autenticacao/login", new
        {
            email = "",
            senha = TestContext.User1Senha
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(400, resp.Status);
    }

    [Fact(DisplayName = "A05 - Refresh token valido retorna novo token")]
    public async Task A05_RefreshToken_Valido()
    {
        var refreshToken = _auth.Client1.RefreshToken;
        Assert.False(string.IsNullOrEmpty(refreshToken));

        var resp = await _auth.Anon.PostAsync("/api/v1/Autenticacao/refresh-token", new
        {
            tokenRefresh = refreshToken
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(200, resp.Status);
        Assert.False(string.IsNullOrEmpty(resp.Data?["tokenAcesso"]?.ToString()));
    }

    [Fact(DisplayName = "A06 - Refresh token invalido retorna 401")]
    public async Task A06_RefreshToken_Invalido()
    {
        var resp = await _auth.Anon.PostAsync("/api/v1/Autenticacao/refresh-token", new
        {
            tokenRefresh = "token_invalido_qa_xyz_000"
        });
        _out.WriteLine(resp.ToString());
        Assert.Equal(401, resp.Status);
    }
}
