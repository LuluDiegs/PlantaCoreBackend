using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

using Microsoft.EntityFrameworkCore;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioPostSave : IRepositorioPostSave
{
    private readonly PlantaCoreDbContext _context;
    public RepositorioPostSave(PlantaCoreDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(PostSave postSave)
    {
        await _context.PostSaves.AddAsync(postSave);
    }

    public async Task RemoverAsync(Guid usuarioId, Guid postId)
    {
        var entity = await _context.PostSaves.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.PostId == postId);
        if (entity != null)
            _context.PostSaves.Remove(entity);
    }

    public async Task<bool> ExisteAsync(Guid usuarioId, Guid postId)
    {
        return await _context.PostSaves.AnyAsync(x => x.UsuarioId == usuarioId && x.PostId == postId);
    }

    public async Task<List<PostSave>> ListarPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.PostSaves.Where(x => x.UsuarioId == usuarioId).ToListAsync();
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
