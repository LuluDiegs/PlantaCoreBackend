using System.Threading.Tasks;
using PlantaCoreAPI.Application.Comuns.Cache;
using PlantaCoreAPI.Application.Comuns.Eventos;

namespace PlantaCoreAPI.Application.Comuns.Eventos;

public static class EventosHandlers
{
    public static void RegistrarTodos(IEventoDispatcher dispatcher, ICacheService cacheService)
    {
        dispatcher.RegistrarHandler<UsuarioSeguidoEvento>(e => InvalidateSeguidorCache(e, cacheService));
        dispatcher.RegistrarHandler<PostCurtidoEvento>(e => InvalidateFeedCache(e, cacheService));
    }

    private static Task InvalidateSeguidorCache(UsuarioSeguidoEvento e, ICacheService cache)
    {
        cache.Remove($"perfil:{e.SeguidorId}");
        cache.Remove($"perfil:{e.SeguidoId}");
        cache.RemoveByPrefix($"feed:{e.SeguidorId}:");
        return Task.CompletedTask;
    }

    private static Task InvalidateFeedCache(PostCurtidoEvento e, ICacheService cache)
    {
        cache.RemoveByPrefix($"feed:{e.UsuarioId}:");
        return Task.CompletedTask;
    }
}
