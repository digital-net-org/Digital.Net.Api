using Digital.Pages.Api.Builders;
using Microsoft.EntityFrameworkCore;
using Digital.Pages.Data.Context;
using Digital.Pages.Services.Seeder;

namespace Digital.Pages.Api;

public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var app = Builder.CreateApp(args);
        app
            .UseCors()
            .UseAuthorization()
            .UseRateLimiter()
            .UseStaticFiles();

        UseSwaggerPage(app);
        await ApplyDataMigrationsAsync(app);
        await SeedDatabaseAsync(app);

        app.MapControllers().RequireRateLimiting("Default");
        await app.RunAsync();
    }

    private static async Task ApplyDataMigrationsAsync(WebApplication app)
    {
        if (app.Environment.IsEnvironment("Test"))
            return;
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DigitalContext>();
        await context.Database.MigrateAsync();
    }

    private static async Task SeedDatabaseAsync(WebApplication app)
    {
        if (!app.Environment.IsEnvironment("Development"))
            return;

        using var scope = app.Services.CreateScope();
        var seederService = scope.ServiceProvider.GetRequiredService<ISeederService>();
        await seederService.SeedDevelopmentDataAsync();
    }

    private static void UseSwaggerPage(WebApplication app)
    {
        if (!app.Environment.IsEnvironment("Development"))
            return;

        app.UseSwagger(opts => { opts.SerializeAsV2 = true; });
        app.UseSwaggerUI(opts =>
        {
            opts.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital.Pages API V1");
            opts.RoutePrefix = "swagger";
        });
    }
}