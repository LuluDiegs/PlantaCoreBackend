using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Notificacao;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Enums;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IRepositorioPlanta _repositorioPlanta;

    public NotificationService(
        IRepositorioNotificacao repositorioNotificacao,
        IRepositorioPlanta repositorioPlanta)
    {
        _repositorioNotificacao = repositorioNotificacao;
        _repositorioPlanta = repositorioPlanta;
    }

    public async Task<Resultado<ListarNotificacoesComLembretesDTOSaida>> ObterNotificacoesAsync(Guid usuarioId)
    {
        try
        {
            var notificacoes = await _repositorioNotificacao.ObterPorUsuarioAsync(usuarioId);
            var totalNaoLidas = await _repositorioNotificacao.ContarNaoLidasAsync(usuarioId);

            var lembretes = notificacoes.Where(n => n.Tipo == TipoNotificacao.LembreteCuidado).ToList();
            var sociais = notificacoes.Where(n => n.Tipo != TipoNotificacao.LembreteCuidado).ToList();

            var lembretesStructurados = new List<LembreteCuidadoDTOSaida>();
            foreach (var lembrete in lembretes)
            {
                if (lembrete.PlantaId.HasValue)
                {
                    var planta = await _repositorioPlanta.ObterPorIdAsync(lembrete.PlantaId.Value);
                    if (planta != null)
                    {
                        lembretesStructurados.Add(new LembreteCuidadoDTOSaida
                        {
                            Id = lembrete.Id,
                            PlantaId = planta.Id,
                            NomePlanta = planta.NomeComum ?? planta.NomeCientifico,
                            DataCriacao = lembrete.DataCriacao,
                            Lida = lembrete.Lida,
                            Detalhes = new LembreteCuidadoDetalhesDTOSaida
                            {
                                Rega = planta.RequisitosAgua,
                                Luz = planta.RequisitosLuz,
                                Temperatura = planta.RequisitosTemperatura,
                                Cuidados = planta.Cuidados
                            }
                        });
                    }
                }
            }

            return Resultado<ListarNotificacoesComLembretesDTOSaida>.Ok(new ListarNotificacoesComLembretesDTOSaida
            {
                NotificacoesSociais = sociais.Select(MapearNotificacaoPara).ToList(),
                Lembretes = lembretesStructurados,
                TotalNaoLidas = totalNaoLidas
            });
        }
        catch (Exception ex)
        {
            return Resultado<ListarNotificacoesComLembretesDTOSaida>.Erro($"Erro ao obter notificações: {ex.Message}");
        }
    }

    public async Task<Resultado<ListarNotificacoesComLembretesDTOSaida>> ObterNaoLidasAsync(Guid usuarioId)
    {
        try
        {
            var notificacoes = await _repositorioNotificacao.ObterNaoLidasAsync(usuarioId);
            var totalNaoLidas = await _repositorioNotificacao.ContarNaoLidasAsync(usuarioId);

            var lembretes = notificacoes.Where(n => n.Tipo == TipoNotificacao.LembreteCuidado).ToList();
            var sociais = notificacoes.Where(n => n.Tipo != TipoNotificacao.LembreteCuidado).ToList();

            var lembretesStructurados = new List<LembreteCuidadoDTOSaida>();
            foreach (var lembrete in lembretes)
            {
                if (lembrete.PlantaId.HasValue)
                {
                    var planta = await _repositorioPlanta.ObterPorIdAsync(lembrete.PlantaId.Value);
                    if (planta != null)
                    {
                        lembretesStructurados.Add(new LembreteCuidadoDTOSaida
                        {
                            Id = lembrete.Id,
                            PlantaId = planta.Id,
                            NomePlanta = planta.NomeComum ?? planta.NomeCientifico,
                            DataCriacao = lembrete.DataCriacao,
                            Lida = lembrete.Lida,
                            Detalhes = new LembreteCuidadoDetalhesDTOSaida
                            {
                                Rega = planta.RequisitosAgua,
                                Luz = planta.RequisitosLuz,
                                Temperatura = planta.RequisitosTemperatura,
                                Cuidados = planta.Cuidados
                            }
                        });
                    }
                }
            }

            return Resultado<ListarNotificacoesComLembretesDTOSaida>.Ok(new ListarNotificacoesComLembretesDTOSaida
            {
                NotificacoesSociais = sociais.Select(MapearNotificacaoPara).ToList(),
                Lembretes = lembretesStructurados,
                TotalNaoLidas = totalNaoLidas
            });
        }
        catch (Exception ex)
        {
            return Resultado<ListarNotificacoesComLembretesDTOSaida>.Erro($"Erro ao obter notificações não lidas: {ex.Message}");
        }
    }

    public async Task<Resultado> MarcarComoLidaAsync(Guid notificacaoId)
    {
        try
        {
            await _repositorioNotificacao.MarcarComoLidaAsync(notificacaoId);
            return Resultado.Ok("Notificação marcada como lida");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao marcar notificação como lida: {ex.Message}");
        }
    }

    public async Task<Resultado> MarcarTodasComoLidasAsync(Guid usuarioId)
    {
        try
        {
            await _repositorioNotificacao.MarcarTodasComoLidasAsync(usuarioId);
            return Resultado.Ok("Todas as notificações marcadas como lidas");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao marcar notificações como lidas: {ex.Message}");
        }
    }

    public async Task<Resultado> DeletarNotificacaoAsync(Guid notificacaoId, Guid usuarioId)
    {
        try
        {
            var notificacao = await _repositorioNotificacao.ObterPorIdAsync(notificacaoId);
            if (notificacao == null)
                return Resultado.Erro("Notificação não encontrada");

            if (notificacao.UsuarioId != usuarioId)
                return Resultado.Erro("Você não tem permissão para deletar esta notificação");

            await _repositorioNotificacao.DeletarNotificacaoAsync(notificacaoId, usuarioId);
            return Resultado.Ok("Notificação deletada com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao deletar notificação: {ex.Message}");
        }
    }

    public async Task<Resultado> DeletarTodasNotificacoesAsync(Guid usuarioId)
    {
        try
        {
            await _repositorioNotificacao.DeletarTodasNotificacoesUsuarioAsync(usuarioId);
            return Resultado.Ok("Todas as notificações foram deletadas");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao deletar notificações: {ex.Message}");
        }
    }

    private static NotificacaoDTOSaida MapearNotificacaoPara(Domain.Entities.Notificacao notificacao)
    {
        return new NotificacaoDTOSaida
        {
            Id = notificacao.Id,
            Tipo = notificacao.Tipo.ToString(),
            Mensagem = notificacao.Mensagem,
            Lida = notificacao.Lida,
            DataCriacao = notificacao.DataCriacao,
            DataLeitura = notificacao.DataLeitura,
            UsuarioOrigemId = notificacao.UsuarioOrigemId,
            UsuarioOrigemNome = notificacao.UsuarioOrigem?.Nome,
            FotoUsuarioOrigem = notificacao.UsuarioOrigem?.FotoPerfil,
            PostId = notificacao.PostId,
            PlantaId = notificacao.PlantaId
        };
    }
}
