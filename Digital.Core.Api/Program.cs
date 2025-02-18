using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Seeds;

namespace Digital.Core.Api;

public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var app = ProgramBuilder.Build(args);
        await app.ApplyMigrationsAsync<DigitalContext>();
        await app.ApplyDataSeedsAsync();

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


}