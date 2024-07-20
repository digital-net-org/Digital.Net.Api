using Microsoft.EntityFrameworkCore;
using SafariDigital.Api.Builders;
using SafariDigital.Database.Context;

namespace SafariDigital.Api;

public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var app = Builder.CreateApp(args);
        app
            .UseSwagger()
            .UseSwaggerUI()
            .UseAuthorization()
            .UseCors()
            .UseRateLimiter();

        if (!app.Environment.IsEnvironment("Test"))
            await ApplyDataMigrationsAsync(app);

        app.MapControllers().RequireRateLimiting("Default");
        await app.RunAsync();
    }

    private static async Task ApplyDataMigrationsAsync(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SafariDigitalContext>();
        await context.Database.MigrateAsync();
    }
}