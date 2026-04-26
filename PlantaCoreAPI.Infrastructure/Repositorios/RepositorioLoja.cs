using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioLoja : IRepositorioLoja
{
    private readonly PlantaCoreDbContext _contexto;

    public RepositorioLoja(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<Loja>> ObterTodosAsync(
        CancellationToken cancellationToken)
    {
        return await _contexto.Lojas.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Loja>> ObterPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        return await _contexto.Lojas
            .Where(l => l.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Loja?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _contexto.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(
        Loja loja,
        CancellationToken cancellationToken)
    {
        await _contexto.Lojas.AddAsync(loja, cancellationToken);
    }

    public void Atualizar(Loja loja)
    {
        _contexto.Attach(loja);
        _contexto.Entry(loja).State = EntityState.Modified;
    }

    public void Remover(Loja loja)
    {
        _contexto.Lojas.Remove(loja);
    }

    public async Task<bool> SalvarMudancasAsync(
        CancellationToken cancellationToken)
    {
        return await _contexto.SaveChangesAsync(cancellationToken) > 0;
    }
}