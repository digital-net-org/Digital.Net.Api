using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Digital.Net.Authentication.Services.Authentication;
using Digital.Net.Core.Http;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Models;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core.Interfaces;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Digital.Net.Tests.Core.Factories;

public class TestApplication : IAsyncInitializer, IAsyncDisposable
{
    private ApplicationFactory? _factory;

    public ApplicationFactory Factory =>
        _factory ?? throw new InvalidOperationException("Application factory not initialized.");

    public Task InitializeAsync()
    {
        _factory = new ApplicationFactory();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
    }

    /// <summary>
    ///     Retrieves a configuration value from the application's service configuration.
    /// </summary>
    public T? GetConfiguration<T>(string key)
    {
        var configuration = Factory.Services.GetService<IConfiguration>();
        return configuration is null ? default : configuration.GetValue<T>(key);
    }

    /// <summary>
    ///     Resolves and retrieves an instance of the specified service from the application's dependency injection container.
    /// </summary>
    public TService GetService<TService>() where TService : notnull => Factory.Services.GetRequiredService<TService>();

    /// <summary>
    ///     Retrieves an instance of the DigitalContext using the application's service
    ///     configuration.
    /// </summary>
    public DigitalContext GetContext() => Factory.Services.GetRequiredService<DigitalContext>();

    /// <summary>
    ///     Creates and configures an HttpClient instance to interact with the application's services.
    /// </summary>
    public HttpClient CreateClient() => Factory.CreateClient();

    /// <summary>
    ///     Creates multiple HttpClient instances.
    /// </summary>
    public List<HttpClient> CreateClient(int amount)
    {
        var result = new List<HttpClient>();
        for (var i = 0; i < amount; i++)
            result.Add(Factory.CreateClient());
        return result;
    }

    /// <summary>
    ///     Creates a test user.
    /// </summary>
    public User CreateUser(TestUserPayload? userDto = null) => GetContext().BuildTestUser(userDto);

    /// <summary>
    ///     Authenticates a user and configures the provided HTTP client with a Bearer token
    ///     using the user's credentials.
    /// </summary>
    public void AsLogged(HttpClient client, User user) =>
        client.AddAuthorization(GetService<IAuthenticationJwtService>().GenerateBearerToken(user.Id, string.Empty));
}