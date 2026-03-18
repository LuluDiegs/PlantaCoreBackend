using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Infrastructure.Services;

public class PlantCareReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlantCareReminderBackgroundService> _logger;
    private bool _executandoAgora = false;

    private static readonly TimeZoneInfo BrasilTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "E. South America Standard Time"
                : "America/Sao_Paulo"
        );

    private static readonly TimeSpan[] HorariosDisparo = new[]
    {
        TimeSpan.FromHours(8)
    };

    public PlantCareReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PlantCareReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[Lembretes] Serviço iniciado. Horários de disparo (Brasil): {Horarios}",
            string.Join(", ", HorariosDisparo.Select(h => h.ToString(@"hh\:mm"))));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var agoraUtc = DateTime.UtcNow;
                var agoraBrasil = TimeZoneInfo.ConvertTimeFromUtc(agoraUtc, BrasilTimeZone);

                var proximoDisparo = ObterProximoDisparo(agoraBrasil);
                var proximoDisparoUtc = TimeZoneInfo.ConvertTimeToUtc(proximoDisparo, BrasilTimeZone);
                var espera = proximoDisparoUtc - agoraUtc;

                if (espera < TimeSpan.Zero)
                    espera = TimeSpan.Zero;

                _logger.LogInformation(
                    "[Lembretes] Agora (Brasil): {Agora:yyyy-MM-dd HH:mm:ss} | Próximo disparo: {Proximo:yyyy-MM-dd HH:mm:ss} | em {Horas:F1}h",
                    agoraBrasil, proximoDisparo, espera.TotalHours);

                await Task.Delay(espera, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    await ExecutarComSegurancaAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[Lembretes] Serviço cancelado.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Lembretes] Erro inesperado no loop principal. Aguardando 5 minutos...");
                await EsperarComCancelamentoAsync(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("[Lembretes] Serviço parado.");
    }

    private async Task ExecutarComSegurancaAsync(CancellationToken stoppingToken)
    {
        if (_executandoAgora)
        {
            _logger.LogWarning("[Lembretes] Execução anterior ainda em andamento. Pulando ciclo.");
            return;
        }

        _executandoAgora = true;
        try
        {
            _logger.LogInformation("[Lembretes] Iniciando geração de lembretes...");
            using var scope = _serviceProvider.CreateScope();
            var servicoLembrete = scope.ServiceProvider.GetRequiredService<IPlantCareReminderService>();

            // Obter todas as plantas com notificações habilitadas
            var plantasComNotificacoes = await servicoLembrete.ObterPlantasComNotificacoesHabilitadasAsync();

            foreach (var planta in plantasComNotificacoes)
            {
                var notificacao = GerarMensagemDeCuidado(planta);
                await servicoLembrete.EnviarNotificacaoAsync(planta.UsuarioId, planta.Id, notificacao);
            }

            _logger.LogInformation("[Lembretes] Lembretes gerados com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Lembretes] Erro ao gerar lembretes. Será tentado novamente no próximo horário.");
        }
        finally
        {
            _executandoAgora = false;
        }
    }

    private string GerarMensagemDeCuidado(Planta planta)
    {
        var mensagens = new List<string>();

        if (!string.IsNullOrWhiteSpace(planta.RequisitosAgua))
            mensagens.Add($"A planta precisa ser regada: {planta.RequisitosAgua}");

        if (!string.IsNullOrWhiteSpace(planta.RequisitosLuz))
            mensagens.Add($"A planta precisa de luz: {planta.RequisitosLuz}");

        if (!string.IsNullOrWhiteSpace(planta.RequisitosTemperatura))
            mensagens.Add($"A planta precisa de temperatura adequada: {planta.RequisitosTemperatura}");

        return string.Join("; ", mensagens);
    }

    private static async Task EsperarComCancelamentoAsync(TimeSpan duracao, CancellationToken stoppingToken)
    {
        try { await Task.Delay(duracao, stoppingToken); }
        catch (OperationCanceledException) { }
    }

    private static DateTime ObterProximoDisparo(DateTime agoraBrasil)
    {
        foreach (var horario in HorariosDisparo.OrderBy(h => h))
        {
            var candidato = agoraBrasil.Date.Add(horario);
            if (candidato > agoraBrasil.AddSeconds(30))
                return candidato;
        }

        return agoraBrasil.Date.AddDays(1).Add(HorariosDisparo.Min());
    }
}