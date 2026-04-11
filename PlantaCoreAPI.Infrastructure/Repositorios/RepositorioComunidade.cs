using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioComunidade : IRepositorioComunidade
{
    private readonly PlantaCoreDbContext _contexto;

    public RepositorioComunidade(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Comunidade?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Comunidades
            .AsTracking()
            .Include(c => c.Criador)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Comunidade>> ObterTodosAsync()
    {
        return await _contexto.Comunidades
            .Include(c => c.Criador)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Comunidade entidade)
    {
        await _contexto.Comunidades.AddAsync(entidade);
    }

    public async Task AtualizarAsync(Comunidade entidade)
    {
        _contexto.Comunidades.Update(entidade);
        await Task.CompletedTask;
    }

    public async Task RemoverAsync(Comunidade entidade)
    {
        _contexto.Comunidades.Remove(entidade);
        await Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Comunidade>> BuscarPorNomeAsync(string termo)
    {
        return await _contexto.Comunidades
            .Where(c => EF.Functions.ILike(c.Nome, $"%{termo}%"))
            .Include(c => c.Criador)
            .Include(c => c.Membros)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<PaginaResultado<Comunidade>> ListarPaginadoAsync(int pagina, int tamanho)
    {
        var query = _contexto.Comunidades
            .Include(c => c.Criador)
            .Include(c => c.Membros);

        var total = await query.CountAsync();
        var itens = await query
            .OrderBy(c => c.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Comunidade>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        };
    }

    public async Task<Comunidade?> ObterComMembrosAsync(Guid comunidadeId)
    {
        return await _contexto.Comunidades
            .AsTracking()
            .Include(c => c.Criador)
            .Include(c => c.Membros)
                .ThenInclude(m => m.Usuario)
            .FirstOrDefaultAsync(c => c.Id == comunidadeId);
    }

    public async Task<bool> UsuarioEhMembroAsync(Guid comunidadeId, Guid usuarioId)
    {
        return await _contexto.MembrosComunidade
            .AnyAsync(m => m.ComunidadeId == comunidadeId && m.UsuarioId == usuarioId);
    }

    public async Task<MembroComunidade?> ObterMembroAsync(Guid comunidadeId, Guid usuarioId)
    {
        return await _contexto.MembrosComunidade
            .AsTracking()
            .FirstOrDefaultAsync(m => m.ComunidadeId == comunidadeId && m.UsuarioId == usuarioId);
    }

    public async Task AdicionarMembroAsync(MembroComunidade membro)
    {
        await _contexto.MembrosComunidade.AddAsync(membro);
    }

    public async Task RemoverMembroAsync(MembroComunidade membro)
    {
        _contexto.MembrosComunidade.Remove(membro);
        await Task.CompletedTask;
    }

    public async Task<PaginaResultado<Comunidade>> ObterComunidadesDoUsuarioAsync(Guid usuarioId, int pagina, int tamanho)
    {
        var query = _contexto.Comunidades
            .Where(c => c.Membros.Any(m => m.UsuarioId == usuarioId))
            .Include(c => c.Criador)
            .Include(c => c.Membros);

        var total = await query.CountAsync();
        var itens = await query
            .OrderBy(c => c.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Comunidade>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        };
    }

    public async Task<IEnumerable<Comunidade>> ListarRecomendadasAsync(int quantidade)
    {
        return await _contexto.Comunidades
            .Include(c => c.Membros)
            .OrderByDescending(c => c.Membros.Count(m => !m.Pendente))
            .ThenByDescending(c => c.DataCriacao)
            .Take(quantidade)
            .ToListAsync();
    }
}
