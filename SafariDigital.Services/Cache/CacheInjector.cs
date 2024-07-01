using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Core.AppSettings;
using SafariDigital.Services.Cache.AttemptCache;
using SafariLib.Core.Environment;
using SafariLib.Jwt;

namespace SafariDigital.Services.Cache;

public static class CacheServiceInjector
{
    public static IServiceCollection AddCacheService(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
        var maxTokenAllowed = configuration?.GetSettingOrThrow<int>(EAppSetting.JwtMaxTokenAllowed) ?? 10;
        services
            .AddMemoryCache()
            .AddJwtCacheService(maxTokenAllowed)
            .AddScoped<IAttemptCacheService, AttemptCacheService>();
        return services;
    }
}