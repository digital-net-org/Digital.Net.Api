using Digital.Net.Core.Bootstrap;
using Digital.Net.Core.Endpoints;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.ApiKeys;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Events;
using Digital.Net.Core.Services.Users;
using Digital.Net.Lib.Configuration;
using Digital.Net.Lib.Environment;
using Digital.Net.Lib.Http;
using Digital.Net.Lib.Origin;
using Digital.Net.Lib.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Digital.Net.Core;

public static class DigitalNetCoreInjector
{
    /// <summary>
    ///     Add the Digital.Net core modules and endpoints to the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddDigitalNetCore(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddAppSettings();
        builder
            .ValidateApplicationSettings()
            .AddDatabaseContext<DigitalContext>()
            .ApplyMigrations<DigitalContext>();

        builder.Services
            .AddDigitalNetLibHttp()
            .AddCrudServices()
            .AddDigitalApiKeyServices()
            .AddDigitalAuditingServices()
            .AddDigitalAuthenticationServices()
            .AddDigitalEventServices()
            .AddDigitalUserServices()
            .AddDigitalDocumentServices()
            .AddHostedService<ExpiredTokenPurgeService>()
            .AddRateLimiter(GlobalLimiter.Options)
            .AddOpenApi();

        builder.Services.RequireContract<IOriginAccessor>(nameof(AddDigitalNetCore));

        builder
            .SetForwardedHeaders()
            .AddDefaultCorsPolicy();

        return builder;
    }

    /// <summary>
    ///     Uses the Digital.Net library
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseDigitalNet(this WebApplication app)
    {
        // Fail-fast strategy, should always be called first
        app.Services.ValidateRequiredContracts();

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
            .MapConfigValueEndpoints()
            .MapValidationEndpoints();

        if (AspNetEnv.IsDevelopment)
        {
            app.MapScalarApiReference();
            app.MapOpenApi();
        }

        return app;
    }

    private static WebApplicationBuilder ValidateApplicationSettings(this WebApplicationBuilder builder)
    {
        var mandatorySettings = new[]
        {
            CoreSettings.DomainKey,
            CoreSettings.ConnectionStringKey
        };

        foreach (var setting in mandatorySettings)
        {
            var value = builder.Configuration.GetSection(setting).Value;
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException($"Missing mandatory configuration section: {setting}");
        }

        return builder;
    }
}