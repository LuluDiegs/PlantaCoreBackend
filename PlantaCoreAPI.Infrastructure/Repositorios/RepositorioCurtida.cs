using Microsoft.EntityFrameworkCore;

using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioCurtida : IRepositorioCurtida
{
    private readonly PlantaCoreDbContext _contexto;
    public RepositorioCurtida(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Curtida?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Curtidas
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Curtida>> ObterTodosAsync()
    {
        return await _contexto.Curtidas.ToListAsync();
    }

    public async Task AdicionarAsync(Curtida entidade)
    {
        await _contexto.Curtidas.AddAsync(entidade);
    }

    public async Task AtualizarAsync(Curtida entidade)
    {
        _contexto.Curtidas.Update(entidade);
        await Task.CompletedTask;
    }

    public async Task RemoverAsync(Curtida entidade)
    {
        _contexto.Curtidas.Remove(entidade);
        await Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExisteAsync(Guid usuarioId, Guid postId)
    {
        return await _contexto.Curtidas.AnyAsync(c => c.UsuarioId == usuarioId && c.PostId == postId);
    }

    public async Task<Curtida?> ObterPorUsuarioEPostAsync(Guid usuarioId, Guid postId)
    {
        return await _contexto.Curtidas
            .AsTracking()
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.PostId == postId);
    }
}
