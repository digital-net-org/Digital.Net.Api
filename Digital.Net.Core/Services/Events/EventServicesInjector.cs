using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Services.Events;

public static class EventServicesInjector
{
    public static IServiceCollection AddDigitalEventServices(this IServiceCollection services)
    {
        services.AddSingleton<IEventSignalService, EventSignalService>();
        return services;
    }
}
