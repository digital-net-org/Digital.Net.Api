using Digital.Net.Api.Services.Pages.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.Pages;

public static class PageServicesInjector
{
    public static IServiceCollection AddDigitalPageServices(this IServiceCollection services)
    {
        services
            .AddScoped<IPageAssetValidationService, PageAssetValidationService>()
            .AddScoped<IPageAssetService, PageAssetService>();
        return services;
    }
}