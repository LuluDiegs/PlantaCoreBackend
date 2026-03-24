using System;
using System.Collections.Concurrent;

namespace PlantaCoreAPI.Application.Comuns.RateLimit;

public interface IRateLimitService
{
    bool IsLimited(string key, int maxRequests, TimeSpan window);
}

public class MemoryRateLimitService : IRateLimitService
{
    private class RateEntry
    {
        public int Count;
        public DateTime WindowStart;
    }

    private readonly ConcurrentDictionary<string, RateEntry> _rates = new();

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
                return false;
            }
            entry.Count++;
            return entry.Count > maxRequests;
        }
    }
}
