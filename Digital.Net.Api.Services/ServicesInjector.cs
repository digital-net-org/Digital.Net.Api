using Digital.Net.Api.Services.Application.Controllers;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.HttpContext;
using Digital.Net.Api.Services.Pages;
using Digital.Net.Api.Services.Users;
using Digital.Net.Api.Services.Validation.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services;

public static class ServicesInjector
{
    /// <summary>
    ///     Registers Digital.Net related services into the provided <see cref="IServiceCollection" />.
    /// </summary>
    public static IServiceCollection AddDigitalNetServices(this IServiceCollection services)
    {
        services
            .AddDigitalHttpContextServices()
            .AddDigitalUserServices()
            .AddDigitalDocumentServices()
            .AddDigitalPageServices();
        return services;
    }

    /// <summary>
    ///     Registers Digital.Net related services into the provided <see cref="WebApplicationBuilder" />.
    /// </summary>
    public static WebApplicationBuilder AddDigitalNetServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDigitalNetServices();
        return builder;
    }

    /// <summary>
    ///     Maps Digital.Net related endpoints into the provided <see cref="IApplicationBuilder" />.
    /// </summary>
    public static WebApplication UseDigitalNetServices(this WebApplication app)
    {
        app
            .MapRootEndpoints()
            .MapValidationEndpoints();
        return app;
    }
}