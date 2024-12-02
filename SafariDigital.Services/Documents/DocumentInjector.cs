using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Services.Documents;

public static class DocumentInjector
{
    public static IServiceCollection AddDocumentServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        return services;
    }
}