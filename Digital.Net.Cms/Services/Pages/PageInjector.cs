using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Cms.Services.Pages;

public static class PageInjector
{
    public static IServiceCollection AddPageDependencies(this IServiceCollection services)
    {
        services.AddScoped<PageCrudService>();
        services.AddScoped<PagePublicService>();

        return services;
    }
}