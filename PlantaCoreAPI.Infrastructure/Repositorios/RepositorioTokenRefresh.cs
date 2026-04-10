using PlantaCoreAPI.Infrastructure.Dados;

using Microsoft.EntityFrameworkCore;

using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioTokenRefresh : IRepositorioTokenRefresh
{
    private readonly PlantaCoreDbContext _contexto;
    public RepositorioTokenRefresh(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<TokenRefresh?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.TokensRefresh.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TokenRefresh>> ObterTodosAsync()
    {
        return await _contexto.TokensRefresh.ToListAsync();
    }

    public async Task AdicionarAsync(TokenRefresh entidade)
    {
        await _contexto.TokensRefresh.AddAsync(entidade);
    }

    public async Task AtualizarAsync(TokenRefresh entidade)
    {
        _contexto.TokensRefresh.Update(entidade);
        await Task.CompletedTask;
    }

    public async Task RemoverAsync(TokenRefresh entidade)
    {
        _contexto.TokensRefresh.Remove(entidade);
        await Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }

    public async Task<TokenRefresh?> ObterPorTokenAsync(string token)
    {
        return await _contexto.TokensRefresh
            .AsTracking()
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task<IEnumerable<TokenRefresh>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _contexto.TokensRefresh
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task RevogarTokensUsuarioAsync(Guid usuarioId)
    {
        var tokens = await _contexto.TokensRefresh
            .AsTracking()
            .Where(t => t.UsuarioId == usuarioId && !t.Revogado)
            .ToListAsync();
        foreach (var token in tokens)
        {
            token.Revogar();
        }

        await SalvarMudancasAsync();
    }
}
