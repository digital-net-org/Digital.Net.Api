using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Digital.Net.Api.Services.HttpContext;

public static class HttpContextServicesInjector
{
    public static IServiceCollection AddDigitalHttpContextServices(this IServiceCollection services)
    {
        services.AddControllers(); // TODO: Should replace with custom Controller attribute
        services.AddScoped<IHttpContextService, HttpContextService>();
        services.AddScoped<IHttpCacheService, HttpCacheService>();
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        return services;
    }
}