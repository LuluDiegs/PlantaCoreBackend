using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Infrastructure.Dados;
using Microsoft.EntityFrameworkCore;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioActivityLog : IRepositorioActivityLog
{
    private readonly PlantaCoreDbContext _context;
    public RepositorioActivityLog(PlantaCoreDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(ActivityLog log)
    {
        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ActivityLog>> ListarPorUsuarioAsync(Guid usuarioId, int pagina = 1, int tamanho = 20)
    {
        return await _context.ActivityLogs
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();
    }
}
