using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PlantaCoreAPI.API.Extensions;

internal static class AuthExtensions
{
    internal static IServiceCollection AddAutenticacaoJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var chaveSecreta = configuration["Jwt:ChaveSecreta"]
            ?? throw new InvalidOperationException("Jwt:ChaveSecreta não configurada.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opcoes =>
            {
                opcoes.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta)),
                    ValidateIssuer = true,
                    ValidIssuer = "PlantaCore",
                    ValidateAudience = true,
                    ValidAudience = "PlantaCoreAPI",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
