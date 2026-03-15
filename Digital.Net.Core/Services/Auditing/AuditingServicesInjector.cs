using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Services.Auditing;

public static class AuditingServicesInjector
{
    public static IServiceCollection AddDigitalAuditingServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}