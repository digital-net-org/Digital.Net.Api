using System.Net;
using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Models.ApiKeys;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Tests.Endpoints.ApiKeyTests;

public class ApiKeyAuthorizationTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    [Test]
    public async Task Authorize_WithValidApiKey_ShouldReturnOk()
    {
        var (_, client) = await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128));
        await ExecuteTestAsync(client, HttpStatusCode.OK);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnExpiredApiKey()
    {
        var (_, client) = await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128),
            DateTime.UtcNow.AddDays(-7));
        await ExecuteTestAsync(client, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnInvalidHeader()
    {
        var (_, client) = await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128), header: "Invalid");
        await ExecuteTestAsync(client, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnInvalidApiKey()
    {
        var client = Application.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApiKeyHeaderAccessor, "SomeString");
        await ExecuteTestAsync(client, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnInactiveUser()
    {
        var (user, client) = await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128));
        var repository = Application.GetRepository<User>();

        var userInDb = await repository.GetByIdAsync(user.Id);
        userInDb!.IsActive = false;
        await repository.UpdateAndSaveAsync(userInDb);
        await ExecuteTestAsync(client, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingApiKeyHeader() =>
        await ExecuteTestAsync(Application.CreateClient(), HttpStatusCode.Unauthorized);

    private async Task<(User, HttpClient)> Setup(string key, DateTime? expiry = null, string? header = null)
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
        await Application
            .GetRepository<ApiKey>()
            .CreateAndSaveAsync(new ApiKey(user.Id, key, expiry));

        client.DefaultRequestHeaders.Add(header ?? AuthenticationStaticOptions.ApiKeyHeaderAccessor, key);
        return (user, client);
    }

    private async Task ExecuteTestAsync(HttpClient client, HttpStatusCode expectedResult)
    {
        var response = await client.TestApiKeyAuthorization();
        await Assert.That(response.StatusCode).EqualTo(expectedResult);
    }
}