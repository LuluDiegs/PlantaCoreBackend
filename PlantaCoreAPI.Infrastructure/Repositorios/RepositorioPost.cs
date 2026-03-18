using PlantaCoreAPI.Infrastructure.Dados;
using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioPost : IRepositorioPost
{
    private readonly PlantaCoreDbContext _contexto;

    public RepositorioPost(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Post?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Posts
            .AsTracking()
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
                .ThenInclude(c => c.Curtidas)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Post>> ObterTodosAsync()
    {
        return await _contexto.Posts
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Post entidade)
    {
        await _contexto.Posts.AddAsync(entidade);
    }

    public async Task AtualizarAsync(Post entidade)
    {
        _contexto.Posts.Update(entidade);
        await Task.CompletedTask;
    }

    public async Task RemoverAsync(Post entidade)
    {
        _contexto.Posts.Remove(entidade);
        await Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Post>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _contexto.Posts
            .Where(p => p.UsuarioId == usuarioId)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> ObterFeedAsync(Guid usuarioId, int pagina = 1, int tamanho = 10)
    {
        var usuario = await _contexto.Usuarios
            .Include(u => u.Seguindo)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null)
            return Enumerable.Empty<Post>();

        var idsUsuariosSeguindo = usuario.Seguindo.Select(u => u.Id).ToList();
        idsUsuariosSeguindo.Add(usuarioId);

        return await _contexto.Posts
            .Where(p => idsUsuariosSeguindo.Contains(p.UsuarioId))
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> ObterPorPlantaAsync(Guid plantaId)
    {
        return await _contexto.Posts
            .Where(p => p.PlantaId == plantaId)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<Post?> ObterPorComentarioIdAsync(Guid comentarioId)
    {
        return await _contexto.Posts
            .AsTracking()
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
                .ThenInclude(c => c.Curtidas)
            .FirstOrDefaultAsync(p => p.Comentarios.Any(c => c.Id == comentarioId));
    }

    public async Task<IEnumerable<Post>> ObterPostsCurtidosPorUsuarioAsync(Guid usuarioId)
    {
        return await _contexto.Posts
            .Where(p => p.Curtidas.Any(c => c.UsuarioId == usuarioId))
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<int> ObterTotalCurtidasRecebidasAsync(Guid usuarioId)
    {
        return await _contexto.Curtidas
            .Where(c => c.Post != null && c.Post.UsuarioId == usuarioId)
            .CountAsync();
    }

    public async Task<Comentario?> ObterComentarioPorIdAsync(Guid comentarioId)
    {
        return await _contexto.Comentarios
            .AsTracking()
            .Include(c => c.Curtidas)
            .FirstOrDefaultAsync(c => c.Id == comentarioId);
    }

    public async Task AtualizarComentarioAsync(Comentario comentario)
    {
        _contexto.Comentarios.Update(comentario);
        await Task.CompletedTask;
    }

    public async Task<PaginaResultado<Post>> ObterPorUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho)
    {
        var query = _contexto.Posts
            .Where(p => p.UsuarioId == usuarioId)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios);

        var total = await query.CountAsync();
        var itens = await query
            .OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Post>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        };
    }

    public async Task<PaginaResultado<Post>> ObterPorComunidadeAsync(Guid comunidadeId, int pagina, int tamanho)
    {
        var query = _contexto.Posts
            .Where(p => p.ComunidadeId == comunidadeId)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios);

        var total = await query.CountAsync();
        var itens = await query
            .OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Post>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        };
    }

    public async Task<PaginaResultado<Post>> ObterExploradorAsync(int pagina, int tamanho)
    {
        var query = _contexto.Posts
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios);

        var total = await query.CountAsync();
        var itens = await query
            .OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Post>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        };
    }

    public async Task<IEnumerable<Post>> ObterPorIdsAsync(IEnumerable<Guid> postIds)
    {
        return await _contexto.Posts
            .Where(p => postIds.Contains(p.Id))
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> ObterPorHashtagAsync(string hashtag)
    {
        return await _contexto.Hashtags
            .Where(h => h.Nome == hashtag)
            .Select(h => h.Post)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> ObterPorCategoriaAsync(string categoria)
    {
        return await _contexto.Categorias
            .Where(c => c.Nome == categoria)
            .Select(c => c.Post)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> ObterPorPalavraChaveAsync(string palavraChave)
    {
        return await _contexto.PalavrasChave
            .Where(pc => pc.Palavra == palavraChave)
            .Select(pc => pc.Post)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .ToListAsync();
    }
}
