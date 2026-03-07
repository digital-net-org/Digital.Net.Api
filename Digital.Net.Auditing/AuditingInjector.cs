using Digital.Net.Auditing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Auditing;

public static class EventServicesInjector
{
    public static IServiceCollection AddDigitalAuditServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}