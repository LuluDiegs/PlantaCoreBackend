using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioRecomendacao : IRepositorioRecomendacao
{
    private readonly PlantaCoreDbContext _contexto;

    public RepositorioRecomendacao(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<Recomendacao>> ObterTodosAsync()
    {
        return await _contexto.Recomendacoes
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Recomendacao>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _contexto.Recomendacoes
            .Where(r => r.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Recomendacao recomendacao)
    {
        await _contexto.Recomendacoes.AddAsync(recomendacao);
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }
}