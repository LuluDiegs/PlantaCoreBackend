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
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
    }

    public async Task<IEnumerable<Post>> ObterTodosAsync()
    {
        return await _contexto.Posts
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Post entidade)
    {
        await _contexto.Posts.AddAsync(entidade);
    }

    public async Task AtualizarAsync(Post entidade)
    {
        if (_contexto.Entry(entidade).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
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
            .Where(p => p.UsuarioId == usuarioId && p.Ativo)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<int> ContarPorUsuarioAsync(Guid usuarioId)
    {
        return await _contexto.Posts
            .CountAsync(p => p.UsuarioId == usuarioId && p.Ativo);
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
            .Where(p => idsUsuariosSeguindo.Contains(p.UsuarioId) && p.Ativo)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
            .OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> ObterPorPlantaAsync(Guid plantaId)
    {
        return await _contexto.Posts
            .Where(p => p.PlantaId == plantaId && p.Ativo)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
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
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
            .FirstOrDefaultAsync(p => p.Comentarios.Any(c => c.Id == comentarioId));
    }

    public async Task<IEnumerable<Post>> ObterPostsCurtidosPorUsuarioAsync(Guid usuarioId)
    {
        return await _contexto.Posts
            .Where(p => p.Ativo && p.Curtidas.Any(c => c.UsuarioId == usuarioId))
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
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

    public async Task<IEnumerable<Post>> ObterPorIdsAsync(IEnumerable<Guid> postIds)
    {
        return await _contexto.Posts
            .Where(p => postIds.Contains(p.Id) && p.Ativo)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> ObterPorHashtagAsync(string hashtag)
    {
        return await _contexto.Hashtags
            .Where(h => h.Nome == hashtag && h.Post.Ativo)
            .Include(h => h.Post)
                .ThenInclude(p => p.Usuario)
            .Include(h => h.Post)
                .ThenInclude(p => p.Curtidas)
            .Include(h => h.Post)
                .ThenInclude(p => p.Comentarios)
            .Select(h => h.Post)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> ObterPorCategoriaAsync(string categoria)
    {
        return await _contexto.Categorias
            .Where(c => c.Nome == categoria && c.Post.Ativo)
            .Include(c => c.Post)
                .ThenInclude(p => p.Usuario)
            .Include(c => c.Post)
                .ThenInclude(p => p.Curtidas)
            .Include(c => c.Post)
                .ThenInclude(p => p.Comentarios)
            .Select(c => c.Post)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> ObterPorPalavraChaveAsync(string palavraChave)
    {
        return await _contexto.PalavrasChave
            .Where(pc => pc.Palavra == palavraChave && pc.Post.Ativo)
            .Include(pc => pc.Post)
                .ThenInclude(p => p.Usuario)
            .Include(pc => pc.Post)
                .ThenInclude(p => p.Curtidas)
            .Include(pc => pc.Post)
                .ThenInclude(p => p.Comentarios)
            .Select(pc => pc.Post)
            .ToListAsync();
    }

    public async Task<PaginaResultado<Post>> ObterPorUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho, string? ordenarPor)
    {
        var query = _contexto.Posts
            .Where(p => p.UsuarioId == usuarioId && p.Ativo)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave);
        var total = await query.CountAsync();
        var itens = await OrdenarPosts(query, ordenarPor)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();
        return new PaginaResultado<Post> { Itens = itens, Pagina = pagina, TamanhoPagina = tamanho, Total = total };
    }

    public async Task<PaginaResultado<Post>> ObterExploradorAsync(int pagina, int tamanho, string? ordenarPor)
    {
        var query = _contexto.Posts
            .Where(p => p.Ativo)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave);
        var total = await query.CountAsync();
        var itens = await OrdenarPosts(query, ordenarPor)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();
        return new PaginaResultado<Post> { Itens = itens, Pagina = pagina, TamanhoPagina = tamanho, Total = total };
    }

    public async Task<PaginaResultado<Post>> ObterPorComunidadeAsync(Guid comunidadeId, int pagina, int tamanho, string? ordenarPor)
    {
        var query = _contexto.Posts
            .Where(p => p.ComunidadeId == comunidadeId && p.Ativo)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave);
        var total = await query.CountAsync();
        var itens = await OrdenarPosts(query, ordenarPor)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();
        return new PaginaResultado<Post> { Itens = itens, Pagina = pagina, TamanhoPagina = tamanho, Total = total };
    }

    public async Task<PaginaResultado<Post>> BuscarPostsAsync(string? q, int pagina, int tamanho)
    {
        var query = _contexto.Posts
            .Where(p => p.Ativo)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
            .Include(p => p.Comunidade)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(p =>
                EF.Functions.ILike(p.Conteudo, "%" + q + "%") ||
                p.Hashtags.Any(h => EF.Functions.ILike(h.Nome, "%" + q + "%")) ||
                p.Categorias.Any(c => EF.Functions.ILike(c.Nome, "%" + q + "%")) ||
                p.PalavrasChave.Any(pc => EF.Functions.ILike(pc.Palavra, "%" + q + "%")) ||
                (p.Usuario != null && EF.Functions.ILike(p.Usuario.Nome, "%" + q + "%")) ||
                (p.Comunidade != null && EF.Functions.ILike(p.Comunidade.Nome, "%" + q + "%")));
        }

        var total = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Post> { Itens = posts, Pagina = pagina, TamanhoPagina = tamanho, Total = total };
    }

    public async Task<PaginaResultado<Post>> BuscarPostsPorPlantaAsync(string nomePlanta, int pagina, int tamanho)
    {
        var query = _contexto.Posts
            .Where(p => p.Ativo && p.Planta != null &&
                (EF.Functions.ILike(p.Planta.NomeCientifico, $"%{nomePlanta}%") ||
                 (p.Planta.NomeComum != null && EF.Functions.ILike(p.Planta.NomeComum, $"%{nomePlanta}%"))))
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Planta)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave);

        var total = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Post> { Itens = posts, Pagina = pagina, TamanhoPagina = tamanho, Total = total };
    }

    public async Task<IEnumerable<Post>> ObterTrendingPostsAsync(int quantidade)
    {
        return await _contexto.Posts
            .Where(p => p.Ativo && p.Usuario != null)
            .Include(p => p.Usuario)
            .Include(p => p.Curtidas)
            .Include(p => p.Comentarios)
            .Include(p => p.Planta)
            .Include(p => p.Hashtags)
            .Include(p => p.Categorias)
            .Include(p => p.PalavrasChave)
            .OrderByDescending(p => p.Curtidas.Count + p.Comentarios.Count)
            .ThenByDescending(p => p.DataCriacao)
            .Take(quantidade)
            .ToListAsync();
    }

    private static IQueryable<Post> OrdenarPosts(IQueryable<Post> query, string? ordenarPor)
    {
        return ordenarPor switch
        {
            "mais_antigo" => query.OrderBy(p => p.DataCriacao),
            "mais_curtido" => query.OrderByDescending(p => p.Curtidas.Count).ThenByDescending(p => p.DataCriacao),
            "mais_comentado" => query.OrderByDescending(p => p.Comentarios.Count).ThenByDescending(p => p.DataCriacao),
            _ => query.OrderByDescending(p => p.DataCriacao),
        };
    }
}
