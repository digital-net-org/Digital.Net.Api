using Digital.Lib.Net.Bootstrap.Defaults;
using Digital.Lib.Net.Core.Application;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Files;
using Digital.Lib.Net.Mvc;
using Digital.Pages.Api.Data;

namespace Digital.Pages.Api;

public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddAppSettings();

        var app = builder
            .ValidateApplicationSettings()
            .SetForwardedHeaders()
            .AddDatabase()
            .AddDefaultCorsPolicy()
            .AddRateLimiter()
            .AddSwagger("Digital.Pages", "v1")
            .AddAuthentication()
            .AddDigitalFilesServices()
            .AddDigitalMvc()
            .Build();

        await app.ApplyMigrationsAsync<DigitalContext>();
        await app.ApplyMigrationsAsync<DigitalPagesContext>();
        // await SeedDatabaseAsync(app);

        app
            .UseCors()
            .UseAuthorization()
            .UseRateLimiter()
            .UseSwagger()
            .UseStaticFiles();
        app
            .MapControllers()
            .RequireRateLimiting("Default");

        await app.RunAsync();
    }

    // private static async Task SeedDatabaseAsync(WebApplication app)
    // {
    //     if (!app.Environment.IsEnvironment("Development"))
    //         return;
    //
    //     using var scope = app.Services.CreateScope();
    //     var seederService = scope.ServiceProvider.GetRequiredService<ISeederService>();
    //     await seederService.SeedDevelopmentDataAsync();
    // }
}