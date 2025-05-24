using Digital.Net.Api.Core.Environment;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Core.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Api.TestUtilities.Integration;

public static class AppFactorySettings
{
    private static string DbPath => Path.Combine(
        Path.GetTempPath(),
        $"sqlite_db_{Randomizer.GenerateRandomString(Randomizer.AnyNumber, 8)}.db"
    );

    public static Dictionary<string, string?> TestSettings => new()
    {
        { ApplicationSettingsAccessor.Domain, "domain.test" },
        { ApplicationSettingsAccessor.ConnectionString, $"Data Source={DbPath}" },
        { ApplicationSettingsAccessor.UseSqlite, "true" }
    };

    public static IWebHostBuilder UseTestConfiguration(this IWebHostBuilder hostBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .AddAppSettings()
            .AddInMemoryCollection(TestSettings)
            .Build();
        hostBuilder.UseConfiguration(configuration);
        return hostBuilder;
    }

    public static IWebHostBuilder UseTestEnvironment(this IWebHostBuilder hostBuilder)
    {
        hostBuilder.UseEnvironment(AspNetEnv.Test);
        return hostBuilder;
    }
}