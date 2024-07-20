using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Services.Cache.AttemptCache;
using SafariDigital.Services.Cache.JwtCache;

namespace SafariDigital.Services.Cache;

public static class CacheServiceInjector
{
    public static IServiceCollection AddCacheService(this IServiceCollection services)
    {
        services
            .AddMemoryCache()
            .AddScoped<IJwtCacheService, JwtCacheService>()
            .AddScoped<IAttemptCacheService, AttemptCacheService>();
        return services;
    }
}