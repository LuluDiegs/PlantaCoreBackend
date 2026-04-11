using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services;

public class PlantCareReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlantCareReminderBackgroundService> _logger;

    public PlantCareReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PlantCareReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PlantCareReminderBackgroundService iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddHours(8);
                if (now >= nextRun)
                    nextRun = nextRun.AddDays(1);
                var delay = nextRun - now;
                _logger.LogInformation("Próxima execução de lembretes em {Delay:hh\\:mm\\:ss}.", delay);
                await Task.Delay(delay, stoppingToken);
                await ExecutarLembretesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no PlantCareReminderBackgroundService. Aguardando 5 minutos.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("PlantCareReminderBackgroundService encerrado.");
    }

    private async Task ExecutarLembretesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var reminderService = scope.ServiceProvider.GetRequiredService<IPlantCareReminderService>();
        try
        {
            _logger.LogInformation("Gerando lembretes de cuidado para todas as plantas.");
            var plantas = await reminderService.ObterPlantasComNotificacoesHabilitadasAsync();
            foreach (var planta in plantas)
            {
                if (stoppingToken.IsCancellationRequested) break;
                try
                {
                    var nomePlanta = planta.NomeComum ?? planta.NomeCientifico;
                    var mensagem = $"Sua planta {nomePlanta} precisa de cuidados hoje!";
                    await reminderService.EnviarNotificacaoAsync(planta.UsuarioId, planta.Id, mensagem);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao gerar lembrete para planta {PlantaId}.", planta.Id);
                }
            }

            _logger.LogInformation("Geração de lembretes concluída.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar lembretes de cuidado.");
        }
    }
}
