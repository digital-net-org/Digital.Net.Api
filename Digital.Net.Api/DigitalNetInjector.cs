using Digital.Net.Api.Bootstrap;
using Digital.Net.Api.Endpoints;
using Digital.Net.Api.RateLimiter.Limiters;
using Digital.Net.Api.Seeds;
using Digital.Net.Core.Settings;
using Digital.Net.Entities;
using Digital.Net.Entities.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Digital.Net.Api;

public static class DigitalSdkInjector
{
    /// <summary>
    ///     Add the Digital.Net core modules and endpoints to the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddDigitalNet(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddAppSettings();
        builder
            .ValidateApplicationSettings()
            .AddDatabaseContext<DigitalContext>()
            .ApplyMigrations<DigitalContext>();

        builder.Services
            .AddHttpContextAccessor()
            .AddDigitalEntities()
            .AddDigitalServices()
            .AddRateLimiter(GlobalLimiter.Options)
            .AddOpenApi();

        builder
            .SetForwardedHeaders()
            .AddDefaultCorsPolicy()
            .AddDataSeeds();

        return builder;
    }

    /// <summary>
    ///     Uses the Digital.Net library
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseDigitalNet(this WebApplication app)
    {
        app
            .UseCors()
            .UseRateLimiter()
            .UseStaticFiles();

        app
            .MapRootEndpoints()
            .MapAuthenticationEndpoints()
            .MapUserEndpoints()
            .MapApiKeyEndpoints()
            .MapAdministrationEndpoints()
            .MapPageEndpoints()
            .MapValidationEndpoints();
        
        app.MapOpenApi(); // TODO: Protect with API Key
        app.MapScalarApiReference();

        return app;
    }
}