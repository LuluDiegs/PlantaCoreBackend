using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Repositorios;

public class RepositorioEvento : IRepositorioEvento
{
    private readonly PlantaCoreDbContext _contexto;

    public RepositorioEvento(PlantaCoreDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Evento?> ObterPorIdAsync(Guid id)
    {
        return await _contexto.Eventos
            .Include(e => e.Participantes)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Evento?> ObterPorTituloAsync(string titulo)
    {
        return await _contexto.Eventos
            .Include(e => e.Participantes)
            .FirstOrDefaultAsync(e => EF.Functions.ILike(e.Titulo, titulo));
    }

    public async Task<List<Evento>> ObterTodosAsync()
    {
        return await _contexto.Eventos
            .Include(e => e.Participantes)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Evento evento)
    {
        await _contexto.Eventos.AddAsync(evento);
    }

    public void Atualizar(Evento evento)
    {
        _contexto.Eventos.Update(evento);
    }

    public void Remover(Evento evento)
    {
        _contexto.Eventos.Remove(evento);
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }
}
