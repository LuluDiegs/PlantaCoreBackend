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

        var minutosValidade = int.Parse(configuration["Jwt:MinutosValidadeTokenAcesso"] ?? "15");

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

                opcoes.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsJsonAsync(new
                            {
                                sucesso = false,
                                mensagem = "Token expirado"
                            });
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsJsonAsync(new
                        {
                            sucesso = false,
                            mensagem = "Token inválido"
                        });
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsJsonAsync(new
                        {
                            sucesso = false,
                            mensagem = "Não autorizado"
                        });
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
