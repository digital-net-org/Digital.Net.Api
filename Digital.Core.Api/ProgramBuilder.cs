using Digital.Core.Api.Seeds.Development;
using Digital.Core.Api.Services.Users;
using Digital.Lib.Net.Authentication;
using Digital.Lib.Net.Bootstrap.Defaults;
using Digital.Lib.Net.Core.Application;
using Digital.Lib.Net.Core.Environment;
using Digital.Lib.Net.Entities;
using Digital.Lib.Net.Entities.Seeds;
using Digital.Lib.Net.Files;
using Digital.Lib.Net.Mvc;

namespace Digital.Core.Api;

public static class ProgramBuilder
{
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddAppSettings();

        return builder
            .ValidateApplicationSettings()
            .AddApplicationServices()
            .AddDatabaseServices()
            .AddDigitalFilesServices()
            .AddDigitalMvc()
            .AddCoreServices()
            .AddDigitalAuthentication()
            .Build();
    }

    private static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder) =>
        builder
            .SetForwardedHeaders()
            .AddDefaultCorsPolicy()
            .AddRateLimiter()
            .AddSwagger("Digital.Core", "v1");

    private static WebApplicationBuilder AddDatabaseServices(this WebApplicationBuilder builder)
    {
        builder.AddDigitalContext();
        if (AspNetEnv.IsDevelopment)
            builder.Services.AddScoped<ISeed, DevelopmentSeed>();
        
        return builder;
    }

    private static WebApplicationBuilder AddCoreServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserService, UserService>();
        return builder;
    }
}