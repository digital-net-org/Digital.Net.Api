using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Services.HttpContextService;

public static class HttpContextInjector
{
    public static IServiceCollection AddHttpContextService(this IServiceCollection services) =>
        services
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddScoped<IHttpContextService, HttpContextService>();
}