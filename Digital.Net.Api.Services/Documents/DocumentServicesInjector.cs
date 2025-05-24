using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.Documents;

public static class DocumentServicesInjector
{
    public static IServiceCollection AddDigitalDocumentServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        return services;
    }
}