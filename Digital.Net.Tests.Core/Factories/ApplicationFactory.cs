using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Digital.Net.Api.Authentication.Services.Authentication;
using Digital.Net.Api.Core.Environment;
using Digital.Net.Api.Core.Http;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Program;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

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
    ///     Retrieves an instance of a repository for the specified entity type, using the application's service
    ///     configuration.
    /// </summary>
    public IRepository<TEntity, DigitalContext> GetRepository<TEntity>() where TEntity : Entity
    {
        var context = Services.GetRequiredService<DigitalContext>();
        var result = new Repository<TEntity, DigitalContext>(context);
        return result;
    }

    /// <summary>
    ///     Creates a test user.
    /// </summary>
    public User CreateUser(TestUserPayload? userDto = null) => GetRepository<User>().BuildTestUser(userDto);
    
    /// <summary>
    ///     Authenticates a user and configures the provided HTTP client with a Bearer token
    ///     using the user's credentials.
    /// </summary>
    public void AsLogged(HttpClient client, User user) =>
        client.AddAuthorization(GetService<IAuthenticationJwtService>().GenerateBearerToken(user.Id, string.Empty));
}