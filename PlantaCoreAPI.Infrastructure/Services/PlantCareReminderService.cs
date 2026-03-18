using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Enums;
using PlantaCoreAPI.Domain.Interfaces;
using System.Text;

namespace PlantaCoreAPI.Infrastructure.Services;

public class PlantCareReminderService : IPlantCareReminderService
{
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IRepositorioNotificacao _repositorioNotificacao;

    public PlantCareReminderService(
        IRepositorioPlanta repositorioPlanta,
        IRepositorioNotificacao repositorioNotificacao)
    {
        _repositorioPlanta = repositorioPlanta;
        _repositorioNotificacao = repositorioNotificacao;
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
    }

    public async Task GerarLembretesParaTodosPlantas()
    {
        var plantas = await _repositorioPlanta.ObterTodosAsync();

        foreach (var planta in plantas)
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
        }

        await _repositorioNotificacao.SalvarMudancasAsync();
    }

    public async Task<IEnumerable<Planta>> ObterPlantasComNotificacoesHabilitadasAsync()
    {
        return await _repositorioPlanta.ObterTodosAsync(); // Ajustar para filtrar plantas com notificações habilitadas
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
        var hoje = DateTime.UtcNow.Date;
        
        var existente = await _repositorioNotificacao.ObterTodosAsync();
        
        return existente.Any(n => 
            n.PlantaId == plantaId && 
            n.Tipo == TipoNotificacao.LembreteCuidado &&
            !n.Lida &&
            n.DataCriacao.Date == hoje);
    }

    private string ConstruirMensagemCuidado(Planta planta)
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
