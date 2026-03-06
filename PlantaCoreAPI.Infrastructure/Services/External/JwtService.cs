using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services.External;

public class JwtService : IJwtService
{
    private readonly string _chaveSecreta;
    private readonly int _minutosValidade;

    public JwtService(string chaveSecreta, int minutosValidade = 15)
    {
        _chaveSecreta = chaveSecreta;
        _minutosValidade = minutosValidade;
    }

    public string GerarTokenAcesso(Guid usuarioId, string email, string nome)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_chaveSecreta));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, nome)
        };

        var token = new JwtSecurityToken(
            issuer: "PlantaCore",
            audience: "PlantaCoreAPI",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_minutosValidade),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GerarTokenRefresh()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    public ClaimsPrincipal? ValidarTokenAcesso(string token)
    {
        try
        {
            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_chaveSecreta));

            var parametros = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = chave,
                ValidateIssuer = true,
                ValidIssuer = "PlantaCore",
                ValidateAudience = true,
                ValidAudience = "PlantaCoreAPI",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = new JwtSecurityTokenHandler().ValidateToken(token, parametros, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
