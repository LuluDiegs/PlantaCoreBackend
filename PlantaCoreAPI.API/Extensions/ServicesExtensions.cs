using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Comuns.Eventos;
using PlantaCoreAPI.Application.Comuns.Cache;
using PlantaCoreAPI.Application.Comuns.RateLimit;
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
        services.AddScoped<IRepositorioComunidade, RepositorioComunidade>();
        services.AddScoped<IRepositorioSolicitacaoSeguir, RepositorioSolicitacaoSeguir>();
        services.AddScoped<IRepositorioEvento, RepositorioEvento>();
        services.AddScoped<IRepositorioPostSave, RepositorioPostSave>();
        services.AddScoped<IRepositorioPostShare, RepositorioPostShare>();
        services.AddScoped<IRepositorioPostView, RepositorioPostView>();
        services.AddScoped<IRepositorioActivityLog, RepositorioActivityLog>();
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
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPlantCareReminderService, PlantCareReminderService>();
        services.AddScoped<IComunidadeService, ComunidadeService>();

        services.AddScoped<IPostService, PostService>();
                
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
                provider.GetRequiredService<IRepositorioPlanta>(),
                provider.GetRequiredService<IRepositorioNotificacao>(),
                provider.GetRequiredService<IRepositorioSolicitacaoSeguir>(),
                provider.GetRequiredService<IFileStorageService>(),
                provider.GetRequiredService<IAccountDeletionService>(),
                provider.GetRequiredService<IAccountReactivationService>(),
                provider.GetRequiredService<IEventoDispatcher>(),
                provider.GetRequiredService<ICacheService>(),
                provider.GetRequiredService<ILogger<UserService>>()));

        services.AddSingleton<IEventoDispatcher, EventoDispatcher>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<IRateLimitService, MemoryRateLimitService>();

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

    internal static void RegistrarEventosHandlers(this IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IEventoDispatcher>();
        var cache = provider.GetRequiredService<ICacheService>();
        EventosHandlers.RegistrarTodos(dispatcher, cache);
    }
}
