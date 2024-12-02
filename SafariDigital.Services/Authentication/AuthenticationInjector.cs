using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SafariDigital.Services.Authentication.Jwt;
using SafariDigital.Services.Authentication.Service;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SafariDigital.Services.Authentication;

public static class AuthenticationInjector
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetService<IConfiguration>()!;

        services
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IAuthenticatedUserService, AuthenticatedUserService>()
            .AddScoped<IJwtService, JwtService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(bearerOptions =>
            {
                bearerOptions.SaveToken = true;
                bearerOptions.RequireHttpsMetadata = false;
                bearerOptions.TokenValidationParameters = configuration.GetTokenParameters();
            });
        return services;
    }

    public static void AddJwtSwagger(this SwaggerGenOptions options)
    {
        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            }
        );
        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "Bearer"
                    },
                    Array.Empty<string>()
                }
            }
        );
    }
}