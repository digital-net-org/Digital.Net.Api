using System;
using System.Net.Http;
using System.Threading.Tasks;
using Digital.Net.Cms.Context;
using Digital.Net.Core;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core.Interfaces;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Digital.Net.Tests.Core.Factories;

public class ApplicationFixture : IAsyncInitializer, IAsyncDisposable
{
    private ApplicationFactory? _factory;

    [ClassDataSource<PostgresFixture>(Shared = SharedType.PerTestSession)]
    public required PostgresFixture Fixture { get; init; }

    public ApplicationFactory Factory =>
        _factory ?? throw new InvalidOperationException("Application factory not initialized.");

    public Task InitializeAsync()
    {
        _factory = new ApplicationFactory(Fixture.ConnectionString);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null) await _factory.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public T? GetConfiguration<T>(string key)
    {
        var configuration = Factory.Services.GetService<IConfiguration>();
        return configuration is null ? default : configuration.GetValue<T>(key);
    }
    
    public TService GetService<TService>() where TService : notnull => Factory.Services.GetRequiredService<TService>();

    /// <summary>
    ///     Retrieves a fresh <see cref="DigitalContext" /> bound to the shared test database.
    ///     A new context is returned per call so concurrent tests never share an EF Core
    ///     change-tracker (DbContext is not thread-safe).
    /// </summary>
    public DigitalContext GetContext()
        => PostgresContextHelper.CreateContext<DigitalContext>(Fixture.ConnectionString);

    /// <summary>
    ///     Retrieves a fresh <see cref="CmsContext" /> bound to the shared test database.
    ///     See <see cref="GetContext" /> for the threading rationale.
    /// </summary>
    public CmsContext GetCmsContext() => PostgresContextHelper.CreateContext<CmsContext>(Fixture.ConnectionString);
    
    /// <summary>
    ///     Creates and configures an HttpClient instance to interact with the application's services with a unique
    ///     connection IP (see <see cref="TestRemoteIpStartupFilter" />).
    /// </summary>
    public HttpClient CreateClient()
    {
        var client = Factory.CreateClient();
        var rng = Random.Shared;
        client.DefaultRequestHeaders.Add(
            TestRemoteIpStartupFilter.Header,
            $"10.{rng.Next(0, 256)}.{rng.Next(0, 256)}.{rng.Next(1, 255)}"
        );
        return client;
    }

    /// <summary>
    ///     Creates an HttpClient pre-configured with the Application key header for Application-authenticated endpoints.
    /// </summary>
    public HttpClient CreateApplicationClient()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            AuthenticationStaticOptions.ApplicationKeyHeaderAccessor,
            GetConfiguration<string>(CoreSettings.ApplicationKeyKey)
        );
        return client;
    }

    /// <summary>
    ///     Creates a test user. The internal DbContext is disposed before returning so the
    ///     Npgsql pooled connection is released — important under parallel test load.
    /// </summary>
    public User CreateUser(TestUserPayload? userDto = null)
    {
        using var ctx = PostgresContextHelper.CreateContext<DigitalContext>(Fixture.ConnectionString);
        return ctx.BuildTestUser(userDto);
    }
}
