using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.Auditing;

public static class AuditingServicesInjector
{
    public static IServiceCollection AddDigitalAuditingServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}