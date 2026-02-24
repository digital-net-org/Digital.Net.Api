using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.HttpContext;
using Digital.Net.Api.Services.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services;

public static class ServicesInjector
{
    /// <summary>
    ///     Registers Digital.Net related services into the provided <see cref="IServiceCollection" />.
    /// </summary>
    public static IServiceCollection AddDigitalServices(this IServiceCollection services)
    {
        services
            .AddDigitalHttpContextServices()
            .AddDigitalUserServices()
            .AddDigitalDocumentServices();
        return services;
    }

    /// <summary>
    ///     Registers Digital.Net related services into the provided <see cref="WebApplicationBuilder" />.
    /// </summary>
    public static WebApplicationBuilder AddDigitalServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDigitalServices();
        return builder;
    }
}