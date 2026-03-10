using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.ApiKeys;

public static class ApiKeyServicesInjector
{
    public static IServiceCollection AddDigitalApiKeyServices(this IServiceCollection services)
    {
        services.AddScoped<IApiKeyService, ApiKeyService>();
        return services;
    }
}
