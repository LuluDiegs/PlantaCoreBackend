using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Repositorios;
using PlantaCoreAPI.Infrastructure.Services;
using PlantaCoreAPI.Infrastructure.Services.External;
using PlantaCoreAPI.Infrastructure.Storage;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.API.Extensions;

internal static class ServicesExtensions
{
    internal static IServiceCollection AddRepositorios(this IServiceCollection services)
    {
        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        services.AddScoped<IRepositorioPlanta, RepositorioPlanta>();
        services.AddScoped<IRepositorioPost, RepositorioPost>();
        services.AddScoped<IRepositorioNotificacao, RepositorioNotificacao>();
        services.AddScoped<IRepositorioTokenRefresh, RepositorioTokenRefresh>();
        services.AddScoped<IRepositorioCurtida, RepositorioCurtida>();
        services.AddScoped<IRepositorioComentario, RepositorioComentario>();
        return services;
    }

    internal static IServiceCollection AddServicosAplicacao(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IAuthenticationService>(provider =>
            new AuthenticationService(
                provider.GetRequiredService<IRepositorioUsuario>(),
                provider.GetRequiredService<IRepositorioTokenRefresh>(),
                provider.GetRequiredService<IJwtService>(),
                provider.GetRequiredService<IEmailService>(),
                provider.GetRequiredService<IPasswordHashService>(),
                configuration));
        
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPlantCareReminderService, PlantCareReminderService>();
        services.AddScoped<IAccountDeletionService, AccountDeletionService>();
        services.AddScoped<IAccountReactivationService>(provider =>
            new AccountReactivationService(
                provider.GetRequiredService<IRepositorioUsuario>(),
                provider.GetRequiredService<IEmailService>(),
                provider.GetRequiredService<IPasswordHashService>(),
                provider.GetRequiredService<PlantaCoreDbContext>(),
                configuration));

        services.AddScoped<IUserService>(provider =>
            new UserService(
                provider.GetRequiredService<IRepositorioUsuario>(),
                provider.GetRequiredService<IRepositorioPost>(),
                provider.GetRequiredService<IRepositorioNotificacao>(),
                provider.GetRequiredService<IFileStorageService>(),
                provider.GetRequiredService<IAccountDeletionService>(),
                provider.GetRequiredService<IAccountReactivationService>()));

        return services;
    }

    internal static IServiceCollection AddServicosExternos(this IServiceCollection services, IConfiguration configuration)
    {
        var chaveSecreta = configuration["Jwt:ChaveSecreta"]!;
        var minutosValidade = int.Parse(configuration["Jwt:MinutosValidadeTokenAcesso"] ?? "15");
        services.AddScoped<IJwtService>(_ => new JwtService(chaveSecreta, minutosValidade));

        services.AddScoped<IEmailService>(_ => new EmailService(
            configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
            int.Parse(configuration["Email:SmtpPort"] ?? "587"),
            configuration["Email:Email"] ?? "",
            configuration["Email:Senha"] ?? ""));

        services.AddHttpClient(nameof(SupabaseFileStorageService), c => c.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<IFileStorageService>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var url = config["Supabase:Url"] ?? "";
            var key = config["Supabase:ChavePublica"] ?? "";
            var http = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(SupabaseFileStorageService));
            return new SupabaseFileStorageService(http, url, key);
        });

        services.AddHttpClient(nameof(PlantService), c => c.Timeout = TimeSpan.FromSeconds(30));

        services.AddHttpClient("PlantNet", c => c.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<IPlantNetService>(provider =>
            new PlantNetService(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient("PlantNet"),
                configuration["PlantNet:ChaveApi"] ?? ""));

        services.AddHttpClient("Trefle", c => c.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<ITrefleService>(provider =>
            new TrefleService(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient("Trefle"),
                configuration["Trefle:ChaveApi"] ?? ""));

        services.AddHttpClient("Gemini", c => c.Timeout = TimeSpan.FromSeconds(60));
        services.AddScoped<IGeminiService>(provider =>
            new GeminiService(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient("Gemini"),
                provider.GetRequiredService<IConfiguration>()));

        return services;
    }
}
