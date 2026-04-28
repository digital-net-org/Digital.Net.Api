using Digital.Net.Core.Bootstrap;
using Digital.Net.Core.Endpoints;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Seeds;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Lib.Environment;
using Digital.Net.Lib.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Digital.Net.Core;

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
            .AddCrudServices()
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
        app.Use(async (context, next) =>
        {
            context.Response.Headers.XContentTypeOptions = "nosniff";
            await next();
        });

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
            .MapValidationEndpoints();

        if (AspNetEnv.IsDevelopment)
        {
            app.MapScalarApiReference();
            app.MapOpenApi();
        }

        return app;
    }
}