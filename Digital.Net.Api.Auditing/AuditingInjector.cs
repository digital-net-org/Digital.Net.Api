using Digital.Net.Api.Auditing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Auditing;

public static class EventServicesInjector
{
    public static IServiceCollection AddDigitalAuditingServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}