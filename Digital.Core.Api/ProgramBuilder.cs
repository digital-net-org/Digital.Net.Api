using Digital.Core.Api.Seeds;
using Digital.Core.Api.Services.Users;
using Digital.Lib.Net.Authentication;
using Digital.Lib.Net.Core.Environment;
using Digital.Lib.Net.Entities.Seeds;
using Digital.Lib.Net.Files;
using Digital.Lib.Net.Mvc;
using Digital.Lib.Net.Sdk;
using Digital.Lib.Net.Sdk.Bootstrap;

namespace Digital.Core.Api;

public static class ProgramBuilder
{
    public static WebApplication Build(string[] args) =>
        WebApplication.CreateBuilder(args)
            .AddDigitalSdk()
            .AddApplicationServices()
            .AddDataSeeds()
            .AddDigitalFilesServices()
            .AddDigitalMvc()
            .AddCoreServices()
            .AddDigitalAuthentication()
            .Build();

    private static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder) =>
        builder
            .SetForwardedHeaders()
            .AddDefaultCorsPolicy()
            .AddRateLimiter()
            .AddSwagger("Digital.Core", "v1");

    private static WebApplicationBuilder AddDataSeeds(this WebApplicationBuilder builder)
    {
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