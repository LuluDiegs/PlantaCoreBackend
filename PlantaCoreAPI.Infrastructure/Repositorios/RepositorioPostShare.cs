using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

using Microsoft.EntityFrameworkCore;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioPostShare : IRepositorioPostShare
{
    private readonly PlantaCoreDbContext _context;
    public RepositorioPostShare(PlantaCoreDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(PostShare postShare)
    {
        await _context.PostShares.AddAsync(postShare);
    }

    public async Task RemoverAsync(Guid usuarioId, Guid postId)
    {
        var entity = await _context.PostShares.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.PostId == postId);
        if (entity != null)
            _context.PostShares.Remove(entity);
    }

    public async Task<bool> ExisteAsync(Guid usuarioId, Guid postId)
    {
        return await _context.PostShares.AnyAsync(x => x.UsuarioId == usuarioId && x.PostId == postId);
    }

    public async Task<List<PostShare>> ListarPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.PostShares.Where(x => x.UsuarioId == usuarioId).ToListAsync();
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
