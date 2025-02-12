using System.Threading.RateLimiting;
using Digital.Lib.Net.Authentication;
using Digital.Lib.Net.Authentication.Options.Jwt;
using Digital.Lib.Net.Core.Application;
using Digital.Lib.Net.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using SafariDigital.Core;
using SafariDigital.Core.Application;
using SafariDigital.Data;
using SafariDigital.Data.Models.ApiKeys;
using SafariDigital.Data.Models.ApiTokens;
using SafariDigital.Data.Models.Events;
using SafariDigital.Data.Models.Users;
using SafariDigital.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SafariDigital.Api.Builders;

public static class Builder
{
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddAppSettings();
        return builder
            .ValidateApplicationSettings()
            .AddAuthentication()
            .SetForwardedHeaders()
            .AddServices()
            .AddSafariDigitalDatabase()
            .AddCorsPolicy()
            .AddRateLimiter()
            .AddControllers()
            .AddSwagger()
            .Build();
    }

    private static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddDigitalMvc();
        return builder;
    }

    private static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration.GetSection<string[]>(ApplicationSettingPath.CorsAllowedOrigins);
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

    private static WebApplicationBuilder AddRateLimiter(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("Default", opts =>
            {
                opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opts.PermitLimit = 1000;
                opts.QueueLimit = 1000;
                opts.Window = TimeSpan.FromMilliseconds(1000);
            });
        });
        return builder;
    }

    private static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsEnvironment("Test") || builder.Environment.IsEnvironment("Production"))
            return builder;

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SafariDigital", Version = "v1.0" });
            c.EnableAnnotations();
            c.OrderActionsBy(
                apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
            c.DocInclusionPredicate((_, api) =>
            {
                if (!api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller))
                    return true;

                if (controller is not null && controller.EndsWith("Pagination"))
                    api.ActionDescriptor.EndpointMetadata.Add(
                        new SwaggerOperationAttribute { Tags = [controller.Replace("Pagination", string.Empty)] }
                    );

                return true;
            });
        });
        return builder;
    }

    private static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        var cfg = builder.Configuration;
        builder.Services.AddDigitalJwtOptions(opts =>
        {
            opts.SetJwtTokenOptions(new JwtTokenOptions
            {
                Secret = cfg.GetSection<string>(ApplicationSettingPath.JwtSecret),
                Issuer = cfg.GetSection<string>(ApplicationSettingPath.JwtIssuer),
                Audience = cfg.GetSection<string>(ApplicationSettingPath.JwtAudience),
                RefreshTokenExpiration = cfg.GetSection<long>(ApplicationSettingPath.JwtRefreshExpiration),
                AccessTokenExpiration = cfg.GetSection<long>(ApplicationSettingPath.JwtBearerExpiration),
                CookieName = cfg.GetSection<string>(ApplicationSettingPath.JwtCookieName),
                ConcurrentSessions = cfg.GetSection<int>(ApplicationSettingPath.JwtConcurrentSessions)
            });
            opts.SetLoginAttemptsOptions(new LoginAttemptsOptions
            {
                AttemptsThreshold = cfg.GetSection<int>(ApplicationSettingPath.JwtMaxAttempts),
                AttemptsThresholdTime = 900000
            });
            opts.SetPasswordOptions(new PasswordOptions
            {
                PasswordRegex = RegularExpressions.Password,
                SaltSize = cfg.GetSection<int>(ApplicationSettingPath.JwtSaltSize)
            });
        });
        builder.Services.AddDigitalJwtAuthentication<User, ApiToken, EventAuthentication>();
        builder.Services.AddDigitalApiKeyAuthentication<User, ApiKey>();
        return builder;
    }

    private static WebApplicationBuilder SetForwardedHeaders(this WebApplicationBuilder builder)
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