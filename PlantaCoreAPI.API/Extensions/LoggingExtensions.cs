using Microsoft.Extensions.Logging;

namespace PlantaCoreAPI.API.Extensions;

internal static class LoggingExtensions
{
    internal static IServiceCollection AddLoggingConfigurado(this IServiceCollection services)
    {
        services.AddLogging(config =>
        {
            config.AddConsole();
            config.AddDebug();
            config.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            config.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.Warning);
            config.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", LogLevel.Warning);
        });

        return services;
    }
}
