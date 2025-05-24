using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Entities;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.ApiKeys;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Models.ApplicationOptions;
using Digital.Net.Api.Entities.Models.Avatars;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Sdk.Bootstrap;
using Digital.Net.Api.Sdk.RateLimiter.Limiters;
using Digital.Net.Api.Sdk.Seeds;
using Digital.Net.Api.Services;
using Digital.Net.Api.Services.Application;
using Digital.Net.Api.Services.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Sdk;

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
            .SetApplicationName("Digital.Net.Api.Rest")
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
            .AddDigitalNetServices();

        builder.Services
            .BuildServiceProvider()
            .GetService<IOptionsService>()?
            .SettingsInit();

        builder
            .SetForwardedHeaders()
            .AddDefaultCorsPolicy()
            .AddSwagger(builder.GetApplicationName(), "v1");
        
        builder.Services.AddRateLimiter(GlobalLimiter.Options);
        builder.AddDataSeeds();

        return builder;
    }

    public static WebApplication UseDigitalSdk(this WebApplication app)
    {
        app
            .UseCors()
            .UseAuthorization()
            .UseRateLimiter()
            .UseSwaggerPage(app.Configuration.GetApplicationName(), "v1")
            .UseStaticFiles();
        app
            .MapControllers()
            .RequireRateLimiting(GlobalLimiter.Policy);
        return app;
    }
}