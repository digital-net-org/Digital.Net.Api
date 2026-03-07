using Digital.Net.Auditing;
using Digital.Net.Authentication;
using Digital.Net.Controllers;
using Digital.Net.Core.Services;
using Digital.Net.Core.Settings;
using Digital.Net.Entities;
using Digital.Net.Entities.Context;
using Digital.Net.Sdk.Bootstrap;
using Digital.Net.Sdk.RateLimiter.Limiters;
using Digital.Net.Sdk.Seeds;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Digital.Net.Sdk;

public static class DigitalSdkInjector
{
    /// <summary>
    ///     Validate application settings, add the DigitalContext and register Entities services for them and register
    ///     AppOptionService and initiate options in database.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddDigitalSdk(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddAppSettings();
        builder
            .ValidateApplicationSettings()
            .AddDatabaseContext<DigitalContext>()
            .ApplyMigrations<DigitalContext>();

        builder.Services
            .AddDigitalEntities()
            .AddDigitalAuthenticationServices()
            .AddDigitalAuditServices()
            .AddDigitalServices();

        builder
            .SetForwardedHeaders()
            .AddDefaultCorsPolicy();

        builder.Services.AddRateLimiter(GlobalLimiter.Options);
        builder.AddDataSeeds();

        builder.Services.AddOpenApi();

        return builder;
    }

    public static WebApplication UseDigitalSdk(this WebApplication app)
    {
        app
            .UseCors()
            .UseRateLimiter()
            .UseStaticFiles();

        app
            .UseDigitalEndpoints();

        app
            .MapControllers()
            .RequireRateLimiting(GlobalLimiter.Policy);

        app.MapOpenApi();
        app.MapScalarApiReference();

        return app;
    }
}