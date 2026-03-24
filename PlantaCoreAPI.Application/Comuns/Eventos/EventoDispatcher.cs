using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantaCoreAPI.Application.Comuns.Eventos;

public interface IEventoDispatcher
{
    Task PublicarAsync(IEventoDominio evento);
    void RegistrarHandler<T>(Func<T, Task> handler) where T : IEventoDominio;
}

public class EventoDispatcher : IEventoDispatcher
{
    private readonly Dictionary<Type, List<Func<IEventoDominio, Task>>> _handlers = new();

    public void RegistrarHandler<T>(Func<T, Task> handler) where T : IEventoDominio
    {
        var tipo = typeof(T);
        if (!_handlers.ContainsKey(tipo))
            _handlers[tipo] = new List<Func<IEventoDominio, Task>>();
        _handlers[tipo].Add(e => handler((T)e));
    }

    public async Task PublicarAsync(IEventoDominio evento)
    {
        var tipo = evento.GetType();
        if (_handlers.TryGetValue(tipo, out var handlers))
        {
            foreach (var handler in handlers)
                await handler(evento);
        }
    }
}
