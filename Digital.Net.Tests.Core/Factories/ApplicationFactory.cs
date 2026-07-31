using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Digital.Net.Core;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Configuration;
using Digital.Net.Lib.Environment;
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
    public const string TestDomain = "domain.test";
    public const string TestOrigin = "https://bo.domain.test";

    private readonly Dictionary<string, string?> _testSettings;

    public ApplicationFactory(string connectionString, IDictionary<string, string?>? settings = null)
    {
        AspNetEnv.Set(AspNetEnv.Test);

        _testSettings = new Dictionary<string, string?>
        {
            { CoreSettings.ApplicationDomainKey, TestDomain },
            { $"{CoreSettings.CorsAllowedOriginsKey}:0", TestOrigin },
            { CoreSettings.ConnectionStringKey, connectionString },
            { CoreSettings.FileSystemPathKey, ".test_storage" },
            { CoreSettings.ApplicationKeyKey, "test-application-secret-key-for-integration-tests" },
            { "Logging:LogLevel:Default", "None" },
            { "Logging:LogLevel:Microsoft", "None" }
        };
        if (settings is null)
            return;
        foreach (var (key, value) in settings)
            _testSettings[key] = value;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(AspNetEnv.Test);
        var configuration = new ConfigurationBuilder()
            .AddAppSettings()
            .AddInMemoryCollection(_testSettings)
            .Build();

        builder.UseConfiguration(configuration);
        builder.ConfigureServices(services => services.AddSingleton<IStartupFilter, TestRemoteIpStartupFilter>());
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
    ///     Opens a session for a user and attaches its cookie to the client, bypassing the login endpoint.
    ///     Returns the session id so tests can tamper with the stored row (expiry, revocation).
    /// </summary>
    public async Task<string> AsLoggedAsync(HttpClient client, User user)
    {
        var sessionId = await GetService<SessionService>().CreateAsync(user.Id, string.Empty);
        client.SetCookie(GetService<AuthenticationOptionService>().CookieName, sessionId);
        return sessionId;
    }
}
