using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Entities.Seeds;

public static class SeedUtils
{
    public static WebApplicationBuilder ApplyDataSeeds(this WebApplicationBuilder builder)
    {
        var seeds = builder.Services
            .BuildServiceProvider()
            .GetRequiredService<IEnumerable<ISeed>>();

        foreach (var seed in seeds)
            seed.ApplySeed().Wait();

        return builder;
    }

    public static async Task ApplyDataSeedsAsync(this WebApplication app)
    {
        var seeds = app.Services
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<IEnumerable<ISeed>>();

        foreach (var seed in seeds)
            await seed.ApplySeed();
    }
}