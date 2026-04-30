using System.Net;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Endpoints.Authorization;

public class ApiKeyAuthorizationTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

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
        var client = ApplicationFixture.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApiKeyHeaderAccessor, "SomeString");
        await ExecuteTestAsync(client, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnInactiveUser()
    {
        var (user, client) = await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128));
        var context = ApplicationFixture.GetContext();

        var userInDb = await context.Users.FindAsync(user.Id);
        userInDb!.IsActive = false;
        context.Users.Update(userInDb);
        await context.SaveChangesAsync();
        await ExecuteTestAsync(client, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingApiKeyHeader() =>
        await ExecuteTestAsync(ApplicationFixture.CreateClient(), HttpStatusCode.Unauthorized);

    private async Task<(User, HttpClient)> Setup(string key, DateTime? expiry = null, string? header = null)
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();

        var context = ApplicationFixture.GetContext();
        await context.ApiKeys.AddAsync(new ApiKey(user.Id, "test-key", key, expiry));
        await context.SaveChangesAsync();

        client.DefaultRequestHeaders.Add(header ?? AuthenticationStaticOptions.ApiKeyHeaderAccessor, key);
        return (user, client);
    }

    private async Task ExecuteTestAsync(HttpClient client, HttpStatusCode expectedResult)
    {
        var response = await client.TestApiKeyAuthorization();
        await Assert.That(response.StatusCode).EqualTo(expectedResult);
    }
}