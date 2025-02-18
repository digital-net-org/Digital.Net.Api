using Digital.Core.Api.Services.Users;
using Digital.Lib.Net.Bootstrap.Defaults;
using Digital.Lib.Net.Core.Application;
using Digital.Lib.Net.Entities;
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
            .SetForwardedHeaders()
            .AddCoreServices()
            .AddDataSeeds()
            .AddDefaultCorsPolicy()
            .AddRateLimiter()
            .AddSwagger("Digital.Core", "v1")
            .AddAuthentication()
            .AddDigitalFilesServices()
            .AddDigitalMvc()
            .Build();
    }

    private static WebApplicationBuilder AddCoreServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddDigitalContext();
        return builder;
    }
}