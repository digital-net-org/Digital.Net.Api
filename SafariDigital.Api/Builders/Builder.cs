using Microsoft.AspNetCore.HttpOverrides;
using SafariDigital.Api.Builders.Injectors;
using SafariDigital.Core.Application;
using SafariDigital.Data;
using SafariDigital.Data.Context;
using SafariDigital.Services.Authentication;
using SafariDigital.Services.HttpContext;
using SafariDigital.Services.Jwt;
using SafariDigital.Services.Users;

namespace SafariDigital.Api.Builders;

public static class Builder
{
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        return builder
            .AddProjectSettings()
            .AddDatabase()
            .SetForwardedHeaders()
            .AddServices()
            .AddCorsPolicy()
            .AddRateLimiter()
            .AddControllers()
            .AddSwagger()
            .Build();
    }

    private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddRepositories()
            .AddHttpContextService()
            .AddJwtService()
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
        var allowedOrigins = builder.Configuration.GetSection<string[]>(EApplicationSetting.CorsAllowedOrigins);
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

    private static WebApplicationBuilder SetForwardedHeaders(
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