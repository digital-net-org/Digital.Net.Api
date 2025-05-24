using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.Application;

public static class ApplicationServicesInjector
{
    public static IServiceCollection AddDigitalApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IApplicationService, ApplicationService>();
        return services;
    }
}