namespace PlantaCoreAPI.Application.Comuns.RateLimit;

public interface IRateLimitService
{
    bool IsLimited(string key, int maxRequests, TimeSpan window);
}
