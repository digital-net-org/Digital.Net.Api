using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Digital.Net.Api.Services.Authentication;
using Digital.Net.Core.Environment;
using Digital.Net.Core.Http;
using Digital.Net.Core.Random;
using Digital.Net.Core.Settings;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Entities.Seeds;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Program;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Tests.Core.Factories;

public class ApplicationFactory : WebApplicationFactory<DigitalProgram>
{
    private readonly Dictionary<string, string?> _testSettings;

    public ApplicationFactory()
    {
        AspNetEnv.Set(AspNetEnv.Test);

        var dbPath = Path.Combine(
            Path.GetTempPath(),
            $"sqlite_db_{Randomizer.GenerateRandomString(Randomizer.AnyNumber, 8)}.db"
        );
        _testSettings = new Dictionary<string, string?>
        {
            { AppSettings.DomainKey, "domain.test" },
            { AppSettings.ConnectionStringKey, $"Data Source={dbPath}" },
            { AppSettings.UseSqliteKey, "true" },
            { "Logging:LogLevel:Default", "None" },
            { "Logging:LogLevel:Microsoft", "None" }
        };
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(AspNetEnv.Test);
        var configuration = new ConfigurationBuilder()
            .AddAppSettings()
            .AddInMemoryCollection(_testSettings)
            .Build();

        builder.UseConfiguration(configuration);
        builder.ConfigureServices(s => { s.AddScoped<ISeed, TestSeed>(); });
    }

    /// <summary>
    ///     Retrieves a configuration value from the application's service configuration.
    /// </summary>
    public T? GetConfiguration<T>(string key)
    {
        var configuration = Services.GetService<IConfiguration>();
        return configuration is null ? default : configuration.GetValue<T>(key);
    }

    /// <summary>
    ///     Resolves and retrieves an instance of the specified service from the application's dependency injection container.
    /// </summary>
    public TService GetService<TService>() where TService : notnull => Services.GetRequiredService<TService>();

    /// <summary>
    ///     Retrieves an instance of the DigitalContext using the application's service
    ///     configuration.
    /// </summary>
    public DigitalContext GetContext() => Services.GetRequiredService<DigitalContext>();

    /// <summary>
    ///     Creates a test user.
    /// </summary>
    public User CreateUser(TestUserPayload? userDto = null) => GetContext().BuildTestUser(userDto);
    
    /// <summary>
    ///     Authenticates a user and configures the provided HTTP client with a Bearer token
    ///     using the user's credentials.
    /// </summary>
    public void AsLogged(HttpClient client, User user) =>
        client.AddAuthorization(GetService<IJwtService>().GenerateBearerToken(user.Id, string.Empty));
}