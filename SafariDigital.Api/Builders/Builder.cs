using Microsoft.AspNetCore.HttpOverrides;
using SafariDigital.Api.Builders.Injectors;
using SafariDigital.Core.Application;
using SafariDigital.Data;
using SafariDigital.Data.Context;
using SafariDigital.Services.AuthenticationService;
using SafariDigital.Services.CacheService;
using SafariDigital.Services.HttpContextService;
using SafariDigital.Services.JwtService;
using SafariDigital.Services.UserService;

namespace SafariDigital.Api.Builders;

public static class Builder
{
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        return builder
            .AddProjectSettings()
            .ValidateApplicationSettings()
            .ConnectDatabase()
            .ConfigureForwardedHeaders()
            .InjectServices()
            .AddCorsPolicy()
            .AddRateLimiter()
            .AddControllers()
            .AddSwagger()
            .Build();
    }

    private static WebApplicationBuilder InjectServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddRepositories()
            .AddHttpContextService()
            .AddJwtService()
            .AddCacheService()
            .AddUserService()
            .AddAuthenticationService();
        return builder;
    }

    private static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        return builder;
    }

    private static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration.GetSectionOrThrow<string[]>(EApplicationSetting.CorsAllowedOrigins);
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        return builder;
    }

    private static WebApplicationBuilder ConfigureForwardedHeaders(
        this WebApplicationBuilder builder
    )
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
        return builder;
    }
}