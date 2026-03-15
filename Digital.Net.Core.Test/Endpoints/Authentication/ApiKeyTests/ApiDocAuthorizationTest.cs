using System.Net;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;

namespace Digital.Net.Core.Test.Endpoints.Authentication.ApiKeyTests;

public class ApiDocAuthorizationTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    private async Task<(string apiKey, HttpClient client)> SetupWithApiKey()
    {
        var user = Application.CreateUser(new TestUserPayload { IsActive = true });
        var plaintextKey = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 128);

        var context = Application.GetContext();
        await context.ApiKeys.AddAsync(new ApiKey(user.Id, "doc-access-key", plaintextKey));
        await context.SaveChangesAsync();

        var client = Application.CreateClient();
        return (plaintextKey, client);
    }

    [Test]
    public async Task OpenApi_ShouldReturnUnauthorized_WithoutApiKey()
    {
        var client = Application.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task OpenApi_ShouldReturnOk_WithValidApiKeyHeader()
    {
        var (apiKey, client) = await SetupWithApiKey();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApiKeyHeaderAccessor, apiKey);

        var response = await client.GetAsync("/openapi/v1.json");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task OpenApi_ShouldReturnOk_WithValidApiKeyQueryParam()
    {
        var (apiKey, client) = await SetupWithApiKey();

        var response = await client.GetAsync($"/openapi/v1.json?api-key={apiKey}");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task OpenApi_ShouldReturnUnauthorized_WithInvalidApiKey()
    {
        var client = Application.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApiKeyHeaderAccessor, "invalid-key");

        var response = await client.GetAsync("/openapi/v1.json");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task OpenApi_ShouldReturnUnauthorized_WithExpiredApiKey()
    {
        var user = Application.CreateUser(new TestUserPayload { IsActive = true });
        var plaintextKey = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 128);

        var context = Application.GetContext();
        await context.ApiKeys.AddAsync(
            new ApiKey(user.Id, "expired-doc-key", plaintextKey, DateTime.UtcNow.AddDays(-1)));
        await context.SaveChangesAsync();

        var client = Application.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApiKeyHeaderAccessor, plaintextKey);

        var response = await client.GetAsync("/openapi/v1.json");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}
