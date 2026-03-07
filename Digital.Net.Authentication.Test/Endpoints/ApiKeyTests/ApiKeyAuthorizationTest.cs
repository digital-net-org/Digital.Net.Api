using System.Net;
using Digital.Net.Authentication.Options;
using Digital.Net.Core.Random;
using Digital.Net.Entities.Models.ApiKeys;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Authentication.Test.Endpoints.ApiKeyTests;

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
        var context = Application.GetContext();

        var userInDb = await context.Users.FindAsync(user.Id);
        userInDb!.IsActive = false;
        context.Users.Update(userInDb);
        await context.SaveChangesAsync();
        await ExecuteTestAsync(client, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingApiKeyHeader() =>
        await ExecuteTestAsync(Application.CreateClient(), HttpStatusCode.Unauthorized);

    private async Task<(User, HttpClient)> Setup(string key, DateTime? expiry = null, string? header = null)
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
        
        var context = Application.GetContext();
        await context.ApiKeys.AddAsync(new ApiKey(user.Id, key, expiry));
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