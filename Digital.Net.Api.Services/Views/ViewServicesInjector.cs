using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.Views;

public static class ViewServicesInjector
{
    public static IServiceCollection AddDigitalViewServices(this IServiceCollection services)
    {
        services
            .AddScoped<IPuckConfigService, PuckConfigService>()
            .AddScoped<IPuckConfigValidationService, PuckConfigValidationService>();
        return services;
    }
}