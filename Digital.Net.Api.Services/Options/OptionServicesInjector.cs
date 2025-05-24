using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.Options;

public static class OptionServicesInjector
{
    public static IServiceCollection AddDigitalOptionsServices(this IServiceCollection services)
    {
        services.AddScoped<IOptionsService, OptionsService>();
        return services;
    }
    
}