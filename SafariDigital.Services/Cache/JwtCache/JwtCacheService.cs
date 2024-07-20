using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SafariDigital.Core.Application;
using SafariDigital.Core.Memory;

namespace SafariDigital.Services.Cache.JwtCache;

public class JwtCacheService(
    IMemoryCache memoryCache,
    IConfiguration configuration
)
    : IJwtCacheService
{
    private readonly int _maxTokenAllowed =
        configuration.GetSettingOrThrow<int>(EApplicationSetting.JwtMaxTokenAllowed);

    public bool IsTokenRegistered(Guid userId, string userAgent, string token)
    {
        var tokenList = memoryCache.TryGetValue<List<(string, string)>>(userId.ToString());
        return tokenList?.Any(t => t.Item1 == userAgent && t.Item2 == token) ?? false;
    }

    public void RegisterToken(Guid userId, string userAgent, string token)
    {
        var key = userId.ToString();
        var tokenList = memoryCache.TryGetValue<List<(string, string)>>(key) ?? [];
        var existingTokenIndex = tokenList.FindIndex(t => t.Item1 == userAgent);

        if (existingTokenIndex != -1)
        {
            tokenList[existingTokenIndex] = (userAgent, token);
        }
        else if (tokenList?.Count >= (_maxTokenAllowed < 1 ? 1 : _maxTokenAllowed))
        {
            tokenList.RemoveAt(0);
            tokenList.Add((userAgent, token));
        }
        else
        {
            tokenList?.Add((userAgent, token));
        }

        memoryCache.Set(key, tokenList);
    }

    public void RevokeAllTokens(Guid userId) => memoryCache.Remove(userId.ToString());

    public void RevokeToken(Guid userId, string userAgent, string token)
    {
        var key = userId.ToString();
        var tokenList = memoryCache.TryGetValue<List<(string, string)>>(key) ?? [];
        var existingTokenIndex = tokenList.FindIndex(t => t.Item1 == userAgent && t.Item2 == token);

        if (existingTokenIndex == -1) return;
        tokenList.RemoveAt(existingTokenIndex);
        memoryCache.Set(key, tokenList);
    }
}