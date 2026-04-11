using System.Collections.Concurrent;

using PlantaCoreAPI.Application.Comuns.Cache;

namespace PlantaCoreAPI.Infrastructure.Services.Cache;

public class MemoryCacheService : ICacheService
{
    private sealed class CacheItem
    {
        public required object Value { get; init; }
        public DateTime Expiration { get; init; }
    }

    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

    public T? Get<T>(string key)
    {
        if (!_cache.TryGetValue(key, out var item)) return default;
        if (item.Expiration > DateTime.UtcNow) return (T)item.Value;
        _cache.TryRemove(key, out _);
        return default;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var exp = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(5));
        _cache[key] = new CacheItem { Value = value!, Expiration = exp };
        if (_cache.Count > 500)
            Purge();
    }

    public void Remove(string key) => _cache.TryRemove(key, out _);

    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in _cache.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
            _cache.TryRemove(key, out _);
    }

    private void Purge()
    {
        var now = DateTime.UtcNow;
        foreach (var kv in _cache.Where(kv => kv.Value.Expiration <= now).Take(50))
            _cache.TryRemove(kv.Key, out _);
    }
}
