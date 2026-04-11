using Microsoft.EntityFrameworkCore;

using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
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
            .AsTracking()
            .Include(e => e.Participantes)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Evento?> ObterPorTituloAsync(string titulo)
    {
        var tituloNormalizado = titulo.ToLower();
        return await _contexto.Eventos
            .Include(e => e.Participantes)
            .FirstOrDefaultAsync(e => e.Titulo.ToLower() == tituloNormalizado);
    }

    public async Task<IEnumerable<Evento>> ObterTodosAsync()
    {
        return await ObterTodosComParticipantesAsync();
    }

    public async Task<List<Evento>> ObterTodosComParticipantesAsync()
    {
        return await _contexto.Eventos
            .Include(e => e.Participantes)
            .ToListAsync();
    }

    public async Task<List<Usuario>> ListarParticipantesAsync(Guid eventoId)
    {
        var evento = await _contexto.Eventos
            .Include(e => e.Participantes)
                .ThenInclude(p => p.Usuario)
            .FirstOrDefaultAsync(e => e.Id == eventoId);
        if (evento == null) return new List<Usuario>();
        return evento.Participantes
            .Select(p => p.Usuario!)
            .Where(u => u != null)
            .ToList();
    }

    public async Task AdicionarAsync(Evento evento)
    {
        await _contexto.Eventos.AddAsync(evento);
    }

    public Task AtualizarAsync(Evento evento)
    {
        _contexto.Eventos.Update(evento);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Evento evento)
    {
        _contexto.Eventos.Remove(evento);
        return Task.CompletedTask;
    }

    public async Task<bool> SalvarMudancasAsync()
    {
        return await _contexto.SaveChangesAsync() > 0;
    }
}
