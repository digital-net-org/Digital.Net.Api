using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Digital.Net.Api.Authentication.Services.Authentication;
using Digital.Net.Api.Core.Environment;
using Digital.Net.Api.Core.Extensions.HttpUtilities;
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

namespace Digital.Net.Tests.Core.Factories;

public class ApplicationFactory : WebApplicationFactory<DigitalProgram>
{
    private readonly string _dbPath;
    private readonly Dictionary<string, string?> _testSettings;

    public ApplicationFactory()
    {
        _dbPath = Path.Combine(
            Path.GetTempPath(),
            $"sqlite_db_{Randomizer.GenerateRandomString(Randomizer.AnyNumber, 8)}.db"
        );
        _testSettings = new Dictionary<string, string?>
        {
            { AppSettings.DomainKey, "domain.test" },
            { AppSettings.ConnectionStringKey, $"Data Source={_dbPath}" },
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

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (File.Exists(_dbPath))
            try
            {
                File.Delete(_dbPath);
            }
            catch
            {
                // Ignore temp file lock errors
            }
    }

    public TService GetService<TService>() where TService : notnull => Services.GetRequiredService<TService>();

    public IRepository<TEntity, DigitalContext> GetRepository<TEntity>() where TEntity : Entity
    {
        var context = Services.GetRequiredService<DigitalContext>();
        var result = new Repository<TEntity, DigitalContext>(context);
        return result;
    }

    public User CreateUser(TestUserPayload? userDto = null) => GetRepository<User>().BuildTestUser(userDto);

    public void AsLogged(HttpClient client, User user) =>
        client.AddAuthorization(GetService<IAuthenticationJwtService>().GenerateBearerToken(user.Id, string.Empty));

    public new HttpClient CreateClient() => base.CreateClient();

    public List<HttpClient> CreateClient(int amount)
    {
        var result = new List<HttpClient>();
        for (var i = 0; i < amount; i++)
            result.Add(CreateClient());
        return result;
    }
}