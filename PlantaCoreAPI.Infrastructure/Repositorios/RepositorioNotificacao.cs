using PlantaCoreAPI.Infrastructure.Dados;

using Microsoft.EntityFrameworkCore;

using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioNotificacao : IRepositorioNotificacao
{
    private readonly PlantaCoreDbContext _contexto;
    public RepositorioNotificacao(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Notificacao?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Notificacoes
            .AsTracking()
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Notificacao>> ObterTodosAsync()
    {
        return await _contexto.Notificacoes.ToListAsync();
    }

    public async Task AdicionarAsync(Notificacao entidade)
    {
        await _contexto.Notificacoes.AddAsync(entidade);
    }

    public async Task AtualizarAsync(Notificacao entidade)
    {
        _contexto.Notificacoes.Update(entidade);
        await Task.CompletedTask;
    }

    public async Task RemoverAsync(Notificacao entidade)
    {
        _contexto.Notificacoes.Remove(entidade);
        await Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Notificacao>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _contexto.Notificacoes
            .Include(n => n.UsuarioOrigem)
            .Where(n => n.UsuarioId == usuarioId && !n.DataDelecao.HasValue)
            .OrderByDescending(n => n.DataCriacao)
            .ToListAsync();
    }

    public async Task<PaginaResultado<Notificacao>> ObterPorUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho)
    {
        var query = _contexto.Notificacoes
            .Include(n => n.UsuarioOrigem)
            .Where(n => n.UsuarioId == usuarioId && !n.DataDelecao.HasValue)
            .OrderByDescending(n => n.DataCriacao);
        var total = await query.CountAsync();
        var itens = await query
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();
        return new PaginaResultado<Notificacao>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        };
    }

    public async Task<IEnumerable<Notificacao>> ObterNaoLidasAsync(Guid usuarioId)
    {
        return await _contexto.Notificacoes
            .Include(n => n.UsuarioOrigem)
            .Where(n => n.UsuarioId == usuarioId && !n.Lida && !n.DataDelecao.HasValue)
            .OrderByDescending(n => n.DataCriacao)
            .ToListAsync();
    }

    public async Task<int> ContarNaoLidasAsync(Guid usuarioId)
    {
        return await _contexto.Notificacoes
            .Where(n => n.UsuarioId == usuarioId && !n.Lida && !n.DataDelecao.HasValue)
            .CountAsync();
    }

    public async Task<bool> ExisteLembreteHojeAsync(Guid plantaId)
    {
        var hoje = DateTime.UtcNow.Date;
        var amanha = hoje.AddDays(1);
        return await _contexto.Notificacoes
            .AnyAsync(n =>
                n.PlantaId == plantaId &&
                n.Tipo == Domain.Enums.TipoNotificacao.LembreteCuidado &&
                n.DataCriacao >= hoje &&
                n.DataCriacao < amanha);
    }

    public async Task MarcarComoLidaAsync(Guid notificacaoId)
    {
        var notificacao = await ObterPorIdAsync(notificacaoId);
        if (notificacao != null)
        {
            notificacao.MarcarComoLida();
            await AtualizarAsync(notificacao);
            await SalvarMudancasAsync();
        }
    }

    public async Task MarcarTodasComoLidasAsync(Guid usuarioId)
    {
        var notificacoes = await _contexto.Notificacoes
            .AsTracking()
            .Where(n => n.UsuarioId == usuarioId && !n.Lida && !n.DataDelecao.HasValue)
            .ToListAsync();
        foreach (var notificacao in notificacoes)
        {
            notificacao.MarcarComoLida();
        }

        await SalvarMudancasAsync();
    }

    public async Task DeletarNotificacaoAsync(Guid notificacaoId, Guid usuarioId)
    {
        var notificacao = await _contexto.Notificacoes
            .AsTracking()
            .FirstOrDefaultAsync(n => n.Id == notificacaoId && n.UsuarioId == usuarioId);
        if (notificacao != null)
        {
            notificacao.Deletar();
            await AtualizarAsync(notificacao);
            await SalvarMudancasAsync();
        }
    }

    public async Task DeletarTodasNotificacoesUsuarioAsync(Guid usuarioId)
    {
        var notificacoes = await _contexto.Notificacoes
            .AsTracking()
            .Where(n => n.UsuarioId == usuarioId && !n.DataDelecao.HasValue)
            .ToListAsync();
        foreach (var notificacao in notificacoes)
        {
            notificacao.Deletar();
        }

        await SalvarMudancasAsync();
    }

    public async Task DeletarTodasDoUsuarioAsync(Guid usuarioId)
    {
        var notificacoes = await _contexto.Notificacoes
            .AsTracking()
            .Where(n => n.UsuarioId == usuarioId)
            .ToListAsync();
        foreach (var notificacao in notificacoes)
        {
            _contexto.Notificacoes.Remove(notificacao);
        }

        await SalvarMudancasAsync();
    }
}
