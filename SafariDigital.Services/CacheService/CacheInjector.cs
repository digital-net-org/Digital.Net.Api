using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Services.CacheService.AttemptCache;
using SafariDigital.Services.CacheService.JwtCache;

namespace SafariDigital.Services.CacheService;

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