using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services;

public class PlantCareReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlantCareReminderBackgroundService> _logger;
    private bool _primeiraExecucao = true;

    private static readonly TimeZoneInfo BrasilTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "E. South America Standard Time"
                : "America/Sao_Paulo"
        );

    public PlantCareReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PlantCareReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PlantCareReminderBackgroundService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var agoraUtc = DateTime.UtcNow;
                var agoraBrasil = TimeZoneInfo.ConvertTimeFromUtc(agoraUtc, BrasilTimeZone);

                var proximoDisparoBrasil = ObterProximoDisparoBrasil(agoraBrasil);
                var proximoDisparoUtc = TimeZoneInfo.ConvertTimeToUtc(proximoDisparoBrasil, BrasilTimeZone);

                var espera = proximoDisparoUtc - agoraUtc;

                if (espera < TimeSpan.Zero)
                    espera = TimeSpan.Zero;

                if (_primeiraExecucao)
                {
                    _logger.LogInformation("Primeira execução: Gerando lembretes AGORA para teste em localhost!");
                    await GerarLembretes(stoppingToken);
                    _primeiraExecucao = false;
                }

                _logger.LogInformation(
                    $"Agora (Brasil): {agoraBrasil:yyyy-MM-dd HH:mm:ss} | " +
                    $"Próximo disparo: {proximoDisparoBrasil:yyyy-MM-dd HH:mm:ss} (horário Brasil) | " +
                    $"em {espera.TotalHours:F1} horas"
                );

                await Task.Delay(espera, stoppingToken);

                await GerarLembretes(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("PlantCareReminderBackgroundService cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar lembretes automaticamente");
                _logger.LogInformation("Aguardando 5 minutos antes de tentar novamente...");

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("PlantCareReminderBackgroundService parado");
    }

    private async Task GerarLembretes(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var servicoLembrete = scope.ServiceProvider.GetRequiredService<IPlantCareReminderService>();

        _logger.LogInformation("Gerando lembretes para todas as plantas...");
        await servicoLembrete.GerarLembretesParaTodosPlantas();
        _logger.LogInformation("Lembretes gerados com sucesso!");
    }

    private static DateTime ObterProximoDisparoBrasil(DateTime agoraBrasil)
    {
        var proximo = agoraBrasil.Date.AddHours(8); 

        if (agoraBrasil >= proximo)
            proximo = proximo.AddDays(1);

        return proximo;
    }
}