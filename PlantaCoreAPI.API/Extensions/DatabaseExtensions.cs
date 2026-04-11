using Microsoft.EntityFrameworkCore;

using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.API.Extensions;

internal static class DatabaseExtensions
{
    internal static IServiceCollection AddBancoDeDados(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' não configurada.");
        services.AddDbContext<PlantaCoreDbContext>(opcoes =>
            opcoes.UseNpgsql(connectionString));
        return services;
    }
}
