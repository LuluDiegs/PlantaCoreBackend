using System.Threading.Tasks;
using PlantaCoreAPI.Application.Comuns.Cache;
using PlantaCoreAPI.Application.Comuns.Eventos;

namespace PlantaCoreAPI.Application.Comuns.Eventos;

public static class EventosHandlers
{
    public static void RegistrarTodos(IEventoDispatcher dispatcher, ICacheService cacheService)
    {
        dispatcher.RegistrarHandler<UsuarioSeguidoEvento>(e => InvalidatePerfilCache(e, cacheService));
        dispatcher.RegistrarHandler<PostCurtidoEvento>(e => InvalidateFeedCache(e, cacheService));
        dispatcher.RegistrarHandler<ComentarioCriadoEvento>(e => InvalidateFeedCache(e, cacheService));
        dispatcher.RegistrarHandler<PlantaIdentificadaEvento>(e => Task.CompletedTask);
        dispatcher.RegistrarHandler<LembreteCuidadoCriadoEvento>(e => Task.CompletedTask);
    }

    private static Task InvalidatePerfilCache(UsuarioSeguidoEvento e, ICacheService cache)
    {
        cache.Remove($"perfil:{e.SeguidorId}");
        cache.Remove($"perfil:{e.SeguidoId}");
        return Task.CompletedTask;
    }

    private static Task InvalidateFeedCache(object _, ICacheService cache)
    {
        // Simples: limpar todo o feed (pode ser otimizado)
        foreach (var key in cache.GetType().GetField("_cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(cache) as System.Collections.IDictionary ?? new System.Collections.Hashtable())
        {
            if (key is System.Collections.DictionaryEntry entry && entry.Key is string k && k.StartsWith("feed:"))
                cache.Remove(k);
        }
        return Task.CompletedTask;
    }
}
