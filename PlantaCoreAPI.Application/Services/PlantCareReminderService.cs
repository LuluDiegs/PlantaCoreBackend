using Microsoft.Extensions.Logging;
using PlantaCoreAPI.Application.Comuns.Eventos;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Enums;
using PlantaCoreAPI.Domain.Interfaces;
using System.Text;

namespace PlantaCoreAPI.Application.Services;

public class PlantCareReminderService : IPlantCareReminderService
{
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IEventoDispatcher _eventoDispatcher;
    private readonly ILogger<PlantCareReminderService> _logger;

    public PlantCareReminderService(
        IRepositorioPlanta repositorioPlanta,
        IRepositorioNotificacao repositorioNotificacao,
        IEventoDispatcher eventoDispatcher,
        ILogger<PlantCareReminderService> logger)
    {
        _repositorioPlanta = repositorioPlanta;
        _repositorioNotificacao = repositorioNotificacao;
        _eventoDispatcher = eventoDispatcher;
        _logger = logger;
    }

    public async Task GerarLembreteCuidadoAsync(Guid plantaId)
    {
        var planta = await _repositorioPlanta.ObterPorIdAsync(plantaId);
        if (planta == null)
            return;
        if (await JaExisteLembreteHojeAsync(plantaId))
            return;
        var mensagem = ConstruirMensagemCuidado(planta);
        var notificacao = Notificacao.Criar(
            planta.UsuarioId,
            TipoNotificacao.LembreteCuidado,
            mensagem,
            usuarioOrigemId: null,
            plantaId: plantaId,
            postId: null);
        await _repositorioNotificacao.AdicionarAsync(notificacao);
        await _repositorioNotificacao.SalvarMudancasAsync();
        await _eventoDispatcher.PublicarAsync(new LembreteCuidadoCriadoEvento { UsuarioId = planta.UsuarioId, PlantaId = plantaId });
    }

    public async Task GerarLembretesParaTodosPlantas()
    {
        const int tamanhoPagina = 100;
        var skip = 0;
        while (true)
        {
            var plantas = (await _repositorioPlanta.ObterTodasParaLembreteAsync(skip, tamanhoPagina)).ToList();
            if (plantas.Count == 0)
                break;
            foreach (var planta in plantas)
            {
                try
                {
                    if (await JaExisteLembreteHojeAsync(planta.Id))
                        continue;
                    var mensagem = ConstruirMensagemCuidado(planta);
                    var notificacao = Notificacao.Criar(
                        planta.UsuarioId,
                        TipoNotificacao.LembreteCuidado,
                        mensagem,
                        usuarioOrigemId: null,
                        plantaId: planta.Id,
                        postId: null);
                    await _repositorioNotificacao.AdicionarAsync(notificacao);
                    await _repositorioNotificacao.SalvarMudancasAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao gerar lembrete para planta {PlantaId}", planta.Id);
                }
            }

            if (plantas.Count < tamanhoPagina)
                break;
            skip += tamanhoPagina;
        }
    }

    public async Task<IEnumerable<Planta>> ObterPlantasComNotificacoesHabilitadasAsync()
    {
        return await _repositorioPlanta.ObterTodasParaLembreteAsync(0, 200);
    }

    public async Task EnviarNotificacaoAsync(Guid usuarioId, Guid plantaId, string mensagem)
    {
        var notificacao = Notificacao.Criar(
            usuarioId,
            TipoNotificacao.LembreteCuidado,
            mensagem,
            usuarioOrigemId: null,
            plantaId: plantaId,
            postId: null);
        await _repositorioNotificacao.AdicionarAsync(notificacao);
        await _repositorioNotificacao.SalvarMudancasAsync();
    }

    private async Task<bool> JaExisteLembreteHojeAsync(Guid plantaId)
    {
        return await _repositorioNotificacao.ExisteLembreteHojeAsync(plantaId);
    }

    private static string ConstruirMensagemCuidado(Planta planta)
    {
        var sb = new StringBuilder();
        var nomePlanta = planta.NomeComum ?? planta.NomeCientifico;
        sb.AppendLine($"{nomePlanta}");
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(planta.RequisitosAgua))
        {
            sb.AppendLine("REGA");
            sb.AppendLine(planta.RequisitosAgua);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(planta.RequisitosLuz))
        {
            sb.AppendLine("LUZ");
            sb.AppendLine(planta.RequisitosLuz);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(planta.RequisitosTemperatura))
        {
            sb.AppendLine("TEMPERATURA");
            sb.AppendLine(planta.RequisitosTemperatura);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(planta.Cuidados))
        {
            sb.AppendLine("CUIDADOS");
            sb.AppendLine(planta.Cuidados);
            sb.AppendLine();
        }

        if (sb.Length == 0)
        {
            sb.AppendLine($"{nomePlanta}");
            sb.AppendLine();
            sb.AppendLine("Verifique os requisitos da planta no seu perfil.");
        }

        return sb.ToString();
    }
}
