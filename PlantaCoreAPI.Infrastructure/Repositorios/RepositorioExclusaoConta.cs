using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioExclusaoConta : IRepositorioExclusaoConta
{
    private readonly PlantaCoreDbContext _contexto;

    public RepositorioExclusaoConta(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<string?>> ObterFotosDasPlantasAsync(Guid usuarioId)
    {
        return await _contexto.Plantas
            .Where(p => p.UsuarioId == usuarioId)
            .Select(p => p.FotoPlanta)
            .ToListAsync();
    }

    public async Task DeletarFotosPlantasAsync(Guid usuarioId)
    {
        await Task.CompletedTask;
    }

    public async Task DeletarSeguidoresAsync(Guid usuarioId)
    {
        var usuario = await _contexto.Usuarios
            .Include(u => u.Seguidores)
            .Include(u => u.Seguindo)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario != null)
        {
            usuario.Seguidores.Clear();
            usuario.Seguindo.Clear();
            await _contexto.SaveChangesAsync();
        }
    }

    public async Task DeletarCurtidasAsync(Guid usuarioId)
    {
        var curtidas = await _contexto.Curtidas
            .Where(c => c.UsuarioId == usuarioId)
            .ToListAsync();

        _contexto.Curtidas.RemoveRange(curtidas);
        await _contexto.SaveChangesAsync();
    }

    public async Task DeletarComentariosAsync(Guid usuarioId)
    {
        var comentarios = await _contexto.Comentarios
            .Where(c => c.UsuarioId == usuarioId)
            .ToListAsync();

        _contexto.Comentarios.RemoveRange(comentarios);
        await _contexto.SaveChangesAsync();
    }

    public async Task DeletarPostsAsync(Guid usuarioId)
    {
        var posts = await _contexto.Posts
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();

        _contexto.Posts.RemoveRange(posts);
        await _contexto.SaveChangesAsync();
    }

    public async Task DeletarPlantasAsync(Guid usuarioId)
    {
        var plantas = await _contexto.Plantas
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();

        _contexto.Plantas.RemoveRange(plantas);
        await _contexto.SaveChangesAsync();
    }

    public async Task DeletarTokensRefreshAsync(Guid usuarioId)
    {
        var tokens = await _contexto.TokensRefresh
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync();

        _contexto.TokensRefresh.RemoveRange(tokens);
        await _contexto.SaveChangesAsync();
    }
}
