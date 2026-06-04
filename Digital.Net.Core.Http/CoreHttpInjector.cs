using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Http.Accessors;
using Digital.Net.Core.Http.Bootstrap;
using Digital.Net.Core.Http.Endpoints;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Core.Http.Services.Documents;
using Digital.Net.Core.Http.Services.Events;
using Digital.Net.Lib.Environment;
using Digital.Net.Lib.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Digital.Net.Core.Http;

public static class CoreHttpInjector
{
    /// <summary>
    ///     Registers all the Digital.Net HTTP adapters (endpoints, authentication, CRUD, SSE). The business layer
    ///     must be registered separately via <c>AddDigitalNetCore()</c>.
    /// </summary>
    public static WebApplicationBuilder AddDigitalNetCoreHttp(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddHttpContextAccessor()
            .AddScoped<IOriginAccessor, HttpOriginAccessor>()
            .AddCrudServices()
            .AddDigitalAuthenticationServices()
            .AddSingleton<ISseStreamService, SseStreamService>()
            .AddScoped<DocumentCacheService>()
            .AddHostedService<ExpiredTokenPurgeService>()
            .AddRateLimiter(GlobalLimiter.Options)
            .AddOpenApi();

        builder.Services
            .RequireContract<DigitalContext>("AddDigitalNetCore");

        builder
            .SetForwardedHeaders()
            .AddDefaultCorsPolicy();

        return builder;
    }

    /// <summary>
    ///     Wires the Digital.Net HTTP pipeline.
    /// </summary>
    public static WebApplication UseDigitalNetCoreHttp(this WebApplication app)
    {
        // Fail-fast strategy, should always be called first: throws if the business layer
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
}