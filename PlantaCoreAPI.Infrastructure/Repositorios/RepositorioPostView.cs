using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

using Microsoft.EntityFrameworkCore;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioPostView : IRepositorioPostView
{
    private readonly PlantaCoreDbContext _context;
    public RepositorioPostView(PlantaCoreDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(PostView postView)
    {
        await _context.PostViews.AddAsync(postView);
    }

    public async Task RemoverAsync(Guid usuarioId, Guid postId)
    {
        var entity = await _context.PostViews.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.PostId == postId);
        if (entity != null)
            _context.PostViews.Remove(entity);
    }

    public async Task<bool> ExisteAsync(Guid usuarioId, Guid postId)
    {
        return await _context.PostViews.AnyAsync(x => x.UsuarioId == usuarioId && x.PostId == postId);
    }

    public async Task<List<PostView>> ListarPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.PostViews.Where(x => x.UsuarioId == usuarioId).ToListAsync();
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
