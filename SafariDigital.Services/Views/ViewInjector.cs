using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Services.Views;

public static class ViewInjector
{
    public static IServiceCollection AddViewService(this IServiceCollection services) =>
        services.AddScoped<IViewService, ViewService>();
}