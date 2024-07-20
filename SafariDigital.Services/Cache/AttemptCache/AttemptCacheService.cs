using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SafariDigital.Core.Application;
using SafariDigital.Core.Memory;

namespace SafariDigital.Services.Cache.AttemptCache;

[ExcludeFromCodeCoverage] // Tested in integration tests
public class AttemptCacheService(IMemoryCache memoryCache, IConfiguration configuration)
    : IAttemptCacheService
{
    private readonly TimeSpan _attemptsWindow = TimeSpan.FromMilliseconds(
        configuration.GetSectionOrThrow<long>(EApplicationSetting.SecurityMaxLoginWindow)
    );

    private readonly int _maxAttemptsAllowed = configuration.GetSectionOrThrow<int>(
        EApplicationSetting.SecurityMaxLoginAttempts
    );

    public void LogAttempt(string login, string ipAddress)
    {
        var key = $"{ipAddress}_{login}";
        var attempts = memoryCache.TryGetValue<List<string>>(key) ?? [];
        attempts.Add(DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
        memoryCache.Set(key, attempts, _attemptsWindow);
    }

    public void ClearAttempts(string login, string ipAddress) =>
        memoryCache.Remove($"{ipAddress}_{login}");

    public bool HasExceededAttempts(string login, string ipAddress) =>
        (memoryCache.TryGetValue<List<string>>($"{ipAddress}_{login}") ?? []).Count
        >= _maxAttemptsAllowed;
}