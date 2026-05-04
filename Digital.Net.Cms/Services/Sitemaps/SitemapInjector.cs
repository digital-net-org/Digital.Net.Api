using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Cms.Services.Sitemaps;

public static class SitemapInjector
{
    public static IServiceCollection AddSitemapDependencies(this IServiceCollection services)
    {
        services.AddScoped<SitemapService>();
        return services;
    }
}
