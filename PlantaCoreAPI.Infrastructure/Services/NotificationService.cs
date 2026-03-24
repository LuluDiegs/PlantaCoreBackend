using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Notificacao;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IRepositorioNotificacao _repositorioNotificacao;

    public NotificationService(IRepositorioNotificacao repositorioNotificacao)
    {
        _repositorioNotificacao = repositorioNotificacao;
    }

    public async Task<Resultado<PaginaResultadoNotificacao>> ObterNotificacoesPaginadasAsync(Guid usuarioId, int pagina, int tamanho)
    {
        var todas = (await _repositorioNotificacao.ObterPorUsuarioAsync(usuarioId)).ToList();
        var total = todas.Count;
        var totalNaoLidas = todas.Count(n => !n.Lida);
        var paginadas = todas.Skip((pagina - 1) * tamanho).Take(tamanho)
            .Select(n => new NotificacaoDTOSaida
            {
                Id = n.Id,
                Tipo = n.Tipo.ToString(),
                Mensagem = n.Mensagem,
                Lida = n.Lida,
                DataCriacao = n.DataCriacao,
                DataLeitura = n.DataLeitura,
                UsuarioOrigemId = n.UsuarioOrigemId,
                UsuarioOrigemNome = n.UsuarioOrigem?.Nome,
                FotoUsuarioOrigem = n.UsuarioOrigem?.FotoPerfil,
                PostId = n.PostId,
                PlantaId = n.PlantaId
            })
            .ToList();
        var totalPaginas = tamanho > 0 ? (int)Math.Ceiling((double)total / tamanho) : 0;
        return Resultado<PaginaResultadoNotificacao>.Ok(new PaginaResultadoNotificacao
        {
            Notificacoes = paginadas,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total,
            TotalPaginas = totalPaginas,
            TotalNaoLidas = totalNaoLidas
        });
    }

    public async Task<Resultado> MarcarComoLidaAsync(Guid notificacaoId)
    {
        await _repositorioNotificacao.MarcarComoLidaAsync(notificacaoId);
        return Resultado.Ok("Notificação marcada como lida");
    }

    public async Task<Resultado> MarcarTodasComoLidasAsync(Guid usuarioId)
    {
        await _repositorioNotificacao.MarcarTodasComoLidasAsync(usuarioId);
        return Resultado.Ok("Todas notificações marcadas como lidas");
    }

    public async Task<Resultado> DeletarNotificacaoAsync(Guid notificacaoId, Guid usuarioId)
    {
        await _repositorioNotificacao.DeletarNotificacaoAsync(notificacaoId, usuarioId);
        return Resultado.Ok("Notificação removida");
    }

    public async Task<Resultado> DeletarTodasNotificacoesAsync(Guid usuarioId)
    {
        await _repositorioNotificacao.DeletarTodasNotificacoesUsuarioAsync(usuarioId);
        return Resultado.Ok("Todas notificações removidas");
    }

    public async Task<Resultado<ConfiguracoesNotificacaoDTOSaida>> ObterConfiguracoesAsync(Guid usuarioId)
    {
        // Retorna configurações padrão
        return Resultado<ConfiguracoesNotificacaoDTOSaida>.Ok(new ConfiguracoesNotificacaoDTOSaida());
    }

    public async Task<Resultado> AtualizarConfiguracoesAsync(Guid usuarioId, ConfiguracoesNotificacaoDTOEntrada entrada)
    {
        // Apenas simula atualização
        return Resultado.Ok("Configurações atualizadas");
    }
}
