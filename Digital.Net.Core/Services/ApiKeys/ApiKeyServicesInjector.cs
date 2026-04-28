using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Services.ApiKeys;

public static class ApiKeyServicesInjector
{
    public static IServiceCollection AddDigitalApiKeyServices(this IServiceCollection services)
    {
        services.AddScoped<ApiKeyService>();
        return services;
    }
}
