using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Infrastructure.Services;

// Ajustando o serviço em background para enviar notificações às 8h e permitir seleção de plantas
public class PlantCareReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public PlantCareReminderBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddHours(8); // Próxima execução às 8h UTC

            if (now > nextRun)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            await Task.Delay(delay, stoppingToken);

            using (var scope = _serviceProvider.CreateScope())
            {
                var reminderService = scope.ServiceProvider.GetRequiredService<IPlantCareReminderService>();
                var plantas = await reminderService.ObterPlantasComNotificacoesHabilitadasAsync();

                foreach (var planta in plantas)
                {
                    var mensagem = $"Sua planta {planta.NomeComum ?? planta.NomeCientifico} precisa de cuidados hoje!";
                    await reminderService.EnviarNotificacaoAsync(planta.UsuarioId, planta.Id, mensagem);
                }
            }
        }
    }
}