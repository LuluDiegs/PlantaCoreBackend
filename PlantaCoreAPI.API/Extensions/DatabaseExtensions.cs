using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Infrastructure.Dados;
using System.Net;
using System.Net.Sockets;

namespace PlantaCoreAPI.API.Extensions;

internal static class DatabaseExtensions
{
    internal static IServiceCollection AddBancoDeDados(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' não configurada.");

        connectionString = ResolverEndpoint(connectionString);

        services.AddDbContext<PlantaCoreDbContext>(opcoes =>
            opcoes.UseNpgsql(connectionString));

        return services;
    }

    private static string ResolverEndpoint(string connectionString)
    {
        try
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
            var host = builder.Host;

            if (string.IsNullOrEmpty(host) || IPAddress.TryParse(host, out _))
                return connectionString;

            var enderecos = Dns.GetHostAddresses(host);

            var ipv4 = enderecos.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            var ipv6 = enderecos.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6);

            var ip = ipv4 ?? ipv6;

            if (ip != null)
                builder.Host = ip.ToString();

            return builder.ToString();
        }
        catch
        {
            return connectionString;
        }
    }
}
