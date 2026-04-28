using System.Collections.Generic;
using System.Net.Http;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Entities.Seeds;
using Digital.Net.Lib.Environment;
using Digital.Net.Lib.Settings;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Http;
using Digital.Net.Tests.Program;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Tests.Core.Factories;

public class ApplicationFactory : WebApplicationFactory<DigitalProgram>
{
    private readonly Dictionary<string, string?> _testSettings;

    public ApplicationFactory(string connectionString)
    {
        AspNetEnv.Set(AspNetEnv.Test);

        _testSettings = new Dictionary<string, string?>
        {
            { AppSettings.DomainKey, "domain.test" },
            { AppSettings.ConnectionStringKey, connectionString },
            { AppSettings.FileSystemPathKey, ".test_storage" },
            { AppSettings.ApplicationKeyKey, "test-application-secret-key-for-integration-tests" },
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
