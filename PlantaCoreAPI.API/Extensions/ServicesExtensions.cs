using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Comuns.Eventos;
using PlantaCoreAPI.Application.Comuns.Cache;
using PlantaCoreAPI.Application.Comuns.RateLimit;
using PlantaCoreAPI.Application.Services;
using PlantaCoreAPI.Application.Services.Plant;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Repositorios;
using PlantaCoreAPI.Infrastructure.Services;
using PlantaCoreAPI.Infrastructure.Services.Cache;
using PlantaCoreAPI.Infrastructure.Services.External;
using PlantaCoreAPI.Infrastructure.Services.RateLimit;
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
        services.AddScoped<IRepositorioExclusaoConta, RepositorioExclusaoConta>();
        services.AddScoped<IRepositorioRecomendacao, RepositorioRecomendacao>();
        return services;
    }

    internal static IServiceCollection AddServicosAplicacao(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPlantCareReminderService, PlantCareReminderService>();
        services.AddScoped<IComunidadeService, ComunidadeService>();
        services.AddScoped<IEventoService, EventoService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IAccountDeletionService, AccountDeletionService>();
        services.AddScoped<IAccountReactivationService, AccountReactivationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<IRateLimitService, MemoryRateLimitService>();
        return services;
    }

    internal static IServiceCollection AddServicosExternos(this IServiceCollection services, IConfiguration configuration)
    {
        var chaveSecreta = configuration["Jwt:ChaveSecreta"]!;
        var minutosValidade = int.Parse(configuration["Jwt:MinutosValidadeTokenAcesso"] ?? "15");
        services.AddScoped<IJwtService>(_ => new JwtService(chaveSecreta, minutosValidade));

        services.AddScoped<IEmailService>(provider => new EmailService(
            configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
            int.Parse(configuration["Email:SmtpPort"] ?? "587"),
            configuration["Email:Email"] ?? "",
            configuration["Email:Senha"] ?? "",
            provider.GetRequiredService<ILogger<EmailService>>()));

        services.AddHttpClient(nameof(SupabaseFileStorageService), c => c.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<IFileStorageService>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var url = config["Supabase:Url"] ?? "";
            var key = config["Supabase:ChavePublica"] ?? "";
            var http = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(SupabaseFileStorageService));
            var logger = provider.GetRequiredService<ILogger<SupabaseFileStorageService>>();
            return new SupabaseFileStorageService(http, url, key, logger);
        });

        services.AddHttpClient("PlantService", c => c.Timeout = TimeSpan.FromSeconds(30));

        services.AddHttpClient("PlantNet", c => c.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<IPlantNetService>(provider =>
            new PlantNetService(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient("PlantNet"),
                configuration["PlantNet:ChaveApi"] ?? "",
                provider.GetRequiredService<ILogger<PlantNetService>>()));

        services.AddHttpClient("Trefle", c => c.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<ITrefleService>(provider =>
            new TrefleService(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient("Trefle"),
                configuration["Trefle:ChaveApi"] ?? ""));

        services.AddHttpClient("Gemini", c => c.Timeout = TimeSpan.FromSeconds(60));
        services.AddScoped<IGeminiService>(provider =>
            new GeminiService(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient("Gemini"),
                provider.GetRequiredService<IConfiguration>(),
                provider.GetRequiredService<ILogger<GeminiService>>()));

        return services;
    }

    internal static void RegistrarEventosHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IEventoDispatcher>(provider =>
        {
            var dispatcher = new EventoDispatcher();
            var cache = provider.GetRequiredService<ICacheService>();
            EventosHandlers.RegistrarTodos(dispatcher, cache);
            return dispatcher;
        });
    }
}
