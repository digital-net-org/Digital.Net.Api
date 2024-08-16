using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Services.Documents;

public static class DocumentInjector
{
    public static IServiceCollection AddDocumentService(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        return services;
    }
}