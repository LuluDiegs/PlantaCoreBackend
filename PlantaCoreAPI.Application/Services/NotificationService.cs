using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Notificacao;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IRepositorioNotificacao _repositorioNotificacao;

    public NotificationService(IRepositorioNotificacao repositorioNotificacao)
    {
        _repositorioNotificacao = repositorioNotificacao;
    }

    public async Task<Resultado<PaginaResultadoNotificacao>> ObterNotificacoesPaginadasAsync(Guid usuarioId, int pagina, int tamanho)
    {
        var paginado = await _repositorioNotificacao.ObterPorUsuarioPaginadoAsync(usuarioId, pagina, tamanho);
        var totalNaoLidas = await _repositorioNotificacao.ContarNaoLidasAsync(usuarioId);
        var paginadas = paginado.Itens
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

        return Resultado<PaginaResultadoNotificacao>.Ok(new PaginaResultadoNotificacao
        {
            Notificacoes = paginadas,
            Pagina = paginado.Pagina,
            TamanhoPagina = paginado.TamanhoPagina,
            Total = paginado.Total,
            TotalPaginas = paginado.TotalPaginas,
            TotalNaoLidas = totalNaoLidas
        });
    }

    public async Task<Resultado<IEnumerable<NotificacaoDTOSaida>>> ObterNaoLidasAsync(Guid usuarioId)
    {
        var notificacoes = await _repositorioNotificacao.ObterNaoLidasAsync(usuarioId);
        var dtos = notificacoes.Select(n => new NotificacaoDTOSaida
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
        });
        return Resultado<IEnumerable<NotificacaoDTOSaida>>.Ok(dtos);
    }

    public async Task<Resultado> MarcarComoLidaAsync(Guid notificacaoId, Guid usuarioId)
    {
        var notificacao = await _repositorioNotificacao.ObterPorIdAsync(notificacaoId);
        if (notificacao is null)
            return Resultado.Erro("Notificação não encontrada");
        if (notificacao.UsuarioId != usuarioId)
            return Resultado.Erro("Sem permissão para marcar esta notificação");
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
        var notificacao = await _repositorioNotificacao.ObterPorIdAsync(notificacaoId);
        if (notificacao is null)
            return Resultado.Erro("Notificação não encontrada");
        if (notificacao.UsuarioId != usuarioId)
            return Resultado.Erro("Sem permissão para deletar esta notificação");
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
        return Resultado<ConfiguracoesNotificacaoDTOSaida>.Ok(new ConfiguracoesNotificacaoDTOSaida());
    }

    public async Task<Resultado> AtualizarConfiguracoesAsync(Guid usuarioId, ConfiguracoesNotificacaoDTOEntrada entrada)
    {
        return Resultado.Ok("Configurações atualizadas");
    }
}
