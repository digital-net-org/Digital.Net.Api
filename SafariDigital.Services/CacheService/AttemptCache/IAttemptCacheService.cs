namespace SafariDigital.Services.CacheService.AttemptCache;

public interface IAttemptCacheService
{
    void LogAttempt(string login, string ipAddress);
    void ClearAttempts(string login, string ipAddress);
    bool HasExceededAttempts(string login, string ipAddress);
}