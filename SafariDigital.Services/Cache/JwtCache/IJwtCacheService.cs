namespace SafariDigital.Services.Cache.JwtCache;

public interface IJwtCacheService
{
    void RevokeToken(Guid uniqueId, string userAgent, string token);
    void RevokeAllTokens(Guid uniqueId);
    void RegisterToken(Guid uniqueId, string userAgent, string token);
    bool IsTokenRegistered(Guid uniqueId, string userAgent, string token);
}