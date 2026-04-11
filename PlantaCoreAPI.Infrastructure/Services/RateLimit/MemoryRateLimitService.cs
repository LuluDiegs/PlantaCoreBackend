using System.Collections.Concurrent;

using PlantaCoreAPI.Application.Comuns.RateLimit;

namespace PlantaCoreAPI.Infrastructure.Services.RateLimit;

public class MemoryRateLimitService : IRateLimitService
{
    private sealed class RateEntry
    {
        public int Count;
        public DateTime WindowStart;
    }

    private readonly ConcurrentDictionary<string, RateEntry> _rates = new();
    private int _writeCount;
    private const int PurgeEvery = 500;

    public bool IsLimited(string key, int maxRequests, TimeSpan window)
    {
        var now = DateTime.UtcNow;
        var entry = _rates.GetOrAdd(key, _ => new RateEntry { Count = 0, WindowStart = now });
        lock (entry)
        {
            if (now - entry.WindowStart > window)
            {
                entry.Count = 1;
                entry.WindowStart = now;
                MaybePurge();
                return false;
            }
            entry.Count++;
            return entry.Count > maxRequests;
        }
    }

    private void MaybePurge()
    {
        if (System.Threading.Interlocked.Increment(ref _writeCount) % PurgeEvery != 0)
            return;

        var now = DateTime.UtcNow;
        foreach (var kv in _rates)
        {
            if (now - kv.Value.WindowStart > TimeSpan.FromMinutes(10))
                _rates.TryRemove(kv.Key, out _);
        }
    }
}
