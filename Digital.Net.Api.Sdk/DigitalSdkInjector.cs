using Digital.Net.Api.Auditing;
using Digital.Net.Api.Authentication;
using Digital.Net.Api.Controllers;
using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Entities;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.ApiKeys;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Models.ApplicationOptions;
using Digital.Net.Api.Entities.Models.Avatars;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Sdk.Bootstrap;
using Digital.Net.Api.Sdk.RateLimiter.Limiters;
using Digital.Net.Api.Sdk.Seeds;
using Digital.Net.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Digital.Net.Api.Sdk;

public static class DigitalSdkInjector
{
    /// <summary>
    ///     Validate application settings, add the DigitalContext and register Entities services for them and register
    ///     AppOptionService and initiate options in database.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddDigitalSdk(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddAppSettings();
        builder
            .ValidateApplicationSettings()
            .AddDatabaseContext<DigitalContext>()
            .ApplyMigrations<DigitalContext>();

        builder.Services
            .AddDigitalEntities<ApiKey>()
            .AddDigitalEntities<ApiToken>()
            .AddDigitalEntities<ApplicationOption>()
            .AddDigitalEntities<Avatar>()
            .AddDigitalEntities<Document>()
            .AddDigitalEntities<Event>()
            .AddDigitalEntities<User>()
            .AddDigitalEntities<Page>()
            .AddDigitalEntities<PageAsset>()
            .AddDigitalEntities<PageMeta>()
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