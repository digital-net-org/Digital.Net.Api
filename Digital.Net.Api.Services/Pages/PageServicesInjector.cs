using Digital.Net.Api.Services.Pages.Validation.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.Pages;

public static class PageServicesInjector
{
    public static IServiceCollection AddDigitalPageServices(this IServiceCollection services)
    {
        services
            .AddScoped<IPageAssetValidationService, PageAssetValidationService>()
            .AddScoped<IPuckConfigValidationService, PuckConfigValidationService>()
            .AddScoped<IPageAssetService, PageAssetService>()
            .AddScoped<IPuckConfigService, PuckConfigService>();
        return services;
    }
}