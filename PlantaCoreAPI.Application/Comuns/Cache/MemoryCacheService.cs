using System;
using System.Collections.Concurrent;

namespace PlantaCoreAPI.Application.Comuns.Cache;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
}

public class MemoryCacheService : ICacheService
{
    private class CacheItem
    {
        public object Value { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }

    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            if (item.Expiration > DateTime.UtcNow)
                return (T)item.Value;
            _cache.TryRemove(key, out _);
        }
        return default;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var exp = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(5));
        _cache[key] = new CacheItem { Value = value!, Expiration = exp };
    }

    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }
}
