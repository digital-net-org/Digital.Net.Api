using Digital.Net.Api.Services.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.Events;

public static class EventServicesInjector
{
    public static IServiceCollection AddDigitalEventServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        return services;
    }
}