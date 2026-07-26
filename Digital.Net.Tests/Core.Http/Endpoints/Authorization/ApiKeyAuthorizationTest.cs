using System.Net;
using System.Net.Http.Json;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authorization;

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

    [Test]
    public async Task CreateApiKey_ShouldReturnUnauthorized_WhenAuthenticatedWithApiKey()
    {
        var (_, client) = await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128));
        var response = await client.PostAsJsonAsync(
            "/user/self/api-key", new { Name = "new-key", ExpiredAt = (DateTime?)null });
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task CreateApiKey_ShouldSucceed_WhenAuthenticatedWithJwt()
    {
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true });
        var client = ApplicationFixture.CreateClient();
        await client.Login(user);
        var response = await client.PostAsJsonAsync(
            "/user/self/api-key", new { Name = "jwt-key", ExpiredAt = (DateTime?)null });
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

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