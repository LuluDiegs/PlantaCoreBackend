using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly PlantaCoreDbContext _contexto;

    public RepositorioUsuario(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<Usuario>> ObterTodosAsync()
    {
        return await _contexto.Usuarios.ToListAsync();
    }

    public async Task AdicionarAsync(Usuario entidade)
    {
        await _contexto.Usuarios.AddAsync(entidade);
    }

    public async Task AtualizarAsync(Usuario entidade)
    {
        _contexto.Usuarios.Update(entidade);
        await Task.CompletedTask;
    }

    public async Task RemoverAsync(Usuario entidade)
    {
        _contexto.Usuarios.Remove(entidade);
        await Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        var emailNormalizado = email.ToLower().Trim();
        return await _contexto.Usuarios
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Email == emailNormalizado);
    }

    public async Task<Usuario?> ObterPorEmailIncluindoInativosAsync(string email)
    {
        var emailNormalizado = email.ToLower().Trim();
        return await _contexto.Usuarios
            .AsTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == emailNormalizado);
    }

    public async Task<bool> EmailJaExisteAsync(string email)
    {
        var emailNormalizado = email.ToLower().Trim();
        return await _contexto.Usuarios.AnyAsync(u => u.Email == emailNormalizado);
    }

    public async Task<Usuario?> ObterComPlantasAsync(Guid usuarioId)
    {
        return await _contexto.Usuarios
            .AsTracking()
            .Include(u => u.Plantas)
            .Include(u => u.Seguidores)
            .Include(u => u.Seguindo)
            .Include(u => u.Posts)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);
    }

    public async Task<PaginaResultado<Usuario>> ObterSeguidoresPaginadoAsync(Guid usuarioId, int pagina, int tamanho)
    {
        var query = _contexto.Usuarios
            .Where(u => u.Seguindo.Any(s => s.Id == usuarioId));

        var total = await query.CountAsync();
        var itens = await query
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Usuario>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        };
    }

    public async Task<PaginaResultado<Usuario>> ObterSeguindoPaginadoAsync(Guid usuarioId, int pagina, int tamanho)
    {
        var query = _contexto.Usuarios
            .Where(u => u.Seguidores.Any(s => s.Id == usuarioId));

        var total = await query.CountAsync();
        var itens = await query
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaResultado<Usuario>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        };
    }

    public async Task<IEnumerable<Usuario>> BuscarPorNomeAsync(string termo)
    {
        return await _contexto.Usuarios
            .Where(u => EF.Functions.ILike(u.Nome, $"%{termo}%"))
            .OrderBy(u => u.Nome)
            .Take(50)
            .ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> ObterSugestoesAsync(IEnumerable<Guid> excluirIds, int quantidade)
    {
        return await _contexto.Usuarios
            .Where(u => !excluirIds.Contains(u.Id))
            .Include(u => u.Seguidores)
            .OrderByDescending(u => u.Seguidores.Count)
            .Take(quantidade * 3)
            .ToListAsync();
    }

    public async Task<bool> UsuarioSegueAsync(Guid seguidorId, Guid seguidoId)
    {
        return await _contexto.Usuarios
            .Where(u => u.Id == seguidorId)
            .SelectMany(u => u.Seguindo)
            .AnyAsync(u => u.Id == seguidoId);
    }

    public async Task<Usuario?> ObterComSeguindoAsync(Guid usuarioId)
    {
        return await _contexto.Usuarios
            .AsTracking()
            .Include(u => u.Seguindo)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);
    }

    public async Task<Usuario?> ObterComSeguindoESeguidoresAsync(Guid usuarioId)
    {
        return await _contexto.Usuarios
            .AsTracking()
            .Include(u => u.Seguindo)
            .Include(u => u.Seguidores)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);
    }

    public async Task<IEnumerable<Usuario>> ObterPorIdsAsync(IEnumerable<Guid> ids)
    {
        var lista = ids.ToList();
        return await _contexto.Usuarios
            .Where(u => lista.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<Usuario?> ObterPorIdTrackedAsync(Guid id)
    {
        return await _contexto.Usuarios
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
