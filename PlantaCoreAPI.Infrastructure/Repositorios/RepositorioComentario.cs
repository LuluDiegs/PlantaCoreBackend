using Microsoft.EntityFrameworkCore;

using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioComentario : IRepositorioComentario
{
    private readonly PlantaCoreDbContext _contexto;
    public RepositorioComentario(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Comentario?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Comentarios
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Comentario>> ObterTodosAsync()
    {
        return await _contexto.Comentarios.ToListAsync();
    }

    public async Task AdicionarAsync(Comentario entidade)
    {
        await _contexto.Comentarios.AddAsync(entidade);
    }

    public async Task AtualizarAsync(Comentario entidade)
    {
        _contexto.Comentarios.Update(entidade);
        await Task.CompletedTask;
    }

    public async Task RemoverAsync(Comentario entidade)
    {
        _contexto.Comentarios.Remove(entidade);
        await Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }
}
