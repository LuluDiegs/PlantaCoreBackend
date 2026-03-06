using PlantaCoreAPI.Infrastructure.Dados;
using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioPlanta : IRepositorioPlanta
{
    private readonly PlantaCoreDbContext _contexto;

    public RepositorioPlanta(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Planta?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Plantas
            .AsTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Planta>> ObterTodosAsync()
    {
        return await _contexto.Plantas.ToListAsync();
    }

    public async Task AdicionarAsync(Planta entidade)
    {
        await _contexto.Plantas.AddAsync(entidade);
    }

    public async Task AtualizarAsync(Planta entidade)
    {
        _contexto.Plantas.Update(entidade);
        await Task.CompletedTask;
    }

    public async Task RemoverAsync(Planta entidade)
    {
        _contexto.Plantas.Remove(entidade);
        await Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }

    public async Task<Planta?> ObterPorNomeCientificoAsync(string nomeCientifico)
    {
        return await _contexto.Plantas
            .AsTracking()
            .FirstOrDefaultAsync(p => p.NomeCientifico.ToLower() == nomeCientifico.ToLower());
    }

    public async Task<IEnumerable<Planta>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _contexto.Plantas
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Planta>> BuscarPorNomeAsync(string termo)
    {
        var termoLower = termo.ToLower();
        return await _contexto.Plantas
            .Where(p => p.NomeCientifico.ToLower().Contains(termoLower) ||
                       (p.NomeComum != null && p.NomeComum.ToLower().Contains(termoLower)))
            .ToListAsync();
    }

    public async Task<PaginaResultado<Planta>> ObterPorUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho)
    {
        var query = _contexto.Plantas.Where(p => p.UsuarioId == usuarioId);

        var total = await query.CountAsync();
        var itens = await query
            .OrderByDescending(p => p.DataIdentificacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Planta>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        };
    }
}
