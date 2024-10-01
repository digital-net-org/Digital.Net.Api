using Microsoft.EntityFrameworkCore;
using SafariDigital.Api.Builders;
using SafariDigital.Data.Context;

namespace SafariDigital.Api;

public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var app = Builder.CreateApp(args);
        app
            .UseCors()
            .UseAuthorization()
            // .UseRateLimiter()
            .UseStaticFiles();

        UseSwaggerPage(app);
        await ApplyDataMigrationsAsync(app);

        app.MapControllers().RequireRateLimiting("Default");
        await app.RunAsync();
    }

    private static async Task ApplyDataMigrationsAsync(WebApplication app)
    {
        if (app.Environment.IsEnvironment("Test")) return;
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SafariDigitalContext>();
        await context.Database.MigrateAsync();
    }

    private static void UseSwaggerPage(WebApplication app)
    {
        if (!app.Environment.IsEnvironment("Development")) return;
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SafariDigital API V1");
            c.RoutePrefix = "swagger";
        });
    }
}