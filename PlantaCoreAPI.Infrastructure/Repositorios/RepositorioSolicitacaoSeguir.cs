using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioSolicitacaoSeguir : IRepositorioSolicitacaoSeguir
{
    private readonly PlantaCoreDbContext _contexto;

    public RepositorioSolicitacaoSeguir(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<SolicitacaoSeguir?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.SolicitacoesSeguir
            .AsTracking()
            .Include(s => s.Solicitante)
            .Include(s => s.Alvo)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<SolicitacaoSeguir>> ObterTodosAsync()
    {
        return await _contexto.SolicitacoesSeguir.ToListAsync();
    }

    public async Task AdicionarAsync(SolicitacaoSeguir entidade)
    {
        await _contexto.SolicitacoesSeguir.AddAsync(entidade);
    }

    public async Task AtualizarAsync(SolicitacaoSeguir entidade)
    {
        _contexto.SolicitacoesSeguir.Update(entidade);
        await Task.CompletedTask;
    }

    public async Task RemoverAsync(SolicitacaoSeguir entidade)
    {
        _contexto.SolicitacoesSeguir.Remove(entidade);
        await Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }

    public async Task<SolicitacaoSeguir?> ObterPendentePorParAsync(Guid solicitanteId, Guid alvoId)
    {
        return await _contexto.SolicitacoesSeguir
            .AsTracking()
            .Include(s => s.Solicitante)
            .FirstOrDefaultAsync(s => s.SolicitanteId == solicitanteId && s.AlvoId == alvoId && s.Pendente);
    }

    public async Task<IEnumerable<SolicitacaoSeguir>> ObterPendentesPorAlvoAsync(Guid alvoId)
    {
        return await _contexto.SolicitacoesSeguir
            .Where(s => s.AlvoId == alvoId && s.Pendente)
            .Include(s => s.Solicitante)
            .OrderByDescending(s => s.DataSolicitacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<SolicitacaoSeguir>> ObterPendentesPorSolicitanteAsync(Guid solicitanteId)
    {
        return await _contexto.SolicitacoesSeguir
            .Where(s => s.SolicitanteId == solicitanteId && s.Pendente)
            .Include(s => s.Alvo)
            .OrderByDescending(s => s.DataSolicitacao)
            .ToListAsync();
    }

    public async Task<bool> ExisteSolicitacaoPendenteAsync(Guid solicitanteId, Guid alvoId)
    {
        return await _contexto.SolicitacoesSeguir
            .AnyAsync(s => s.SolicitanteId == solicitanteId && s.AlvoId == alvoId && s.Pendente);
    }
}
