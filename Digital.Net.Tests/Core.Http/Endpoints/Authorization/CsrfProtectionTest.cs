using System.Net;
using System.Net.Http.Json;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authorization;

public class CsrfProtectionTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    [Arguments("POST")]
    [Arguments("PUT")]
    [Arguments("PATCH")]
    [Arguments("DELETE")]
    public async Task SessionMutation_WithoutTheHeader_ShouldBeForbidden(string method)
    {
        var client = await CreateLoggedClientAsync(false);

        var response = await client.TestSessionMutation(method);

        // 403, not 401: this is a transport bug, not an expired session — a 401 would log the operator out.
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    [Arguments("POST")]
    [Arguments("PUT")]
    [Arguments("PATCH")]
    [Arguments("DELETE")]
    public async Task SessionMutation_WithTheHeader_ShouldBeAllowed(string method)
    {
        var client = await CreateLoggedClientAsync(true);

        var response = await client.TestSessionMutation(method);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task SessionRead_WithoutTheHeader_ShouldBeAllowed()
    {
        // Safe methods are exempt: an <img> tag or an EventSource cannot send a custom header.
        var client = await CreateLoggedClientAsync(false);

        var response = await client.TestSessionAuthorization();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task ApiKeyMutation_WithoutTheHeader_ShouldBeAllowed()
    {
        // Machine-to-machine: no ambient authority to protect, and requiring the header would break clients.
        var client = ApplicationFixture.CreateClientWithoutCsrfHeader();
        var user = ApplicationFixture.CreateUser();
        var apiKey = Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128);
        var context = ApplicationFixture.GetContext();
        await context.ApiKeys.AddAsync(new ApiKey(user.Id, "csrf-key", apiKey));
        await context.SaveChangesAsync();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApiKeyHeaderAccessor, apiKey);

        var response = await client.TestApiKeyMutation();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task ApplicationMutation_WithoutTheHeader_ShouldBeAllowed()
    {
        // The public Nuxt site authenticates this way and sends no custom header.
        var response = await ApplicationFixture.CreateApplicationClient().TestApplicationMutation();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Login_WithoutTheHeader_ShouldBeForbidden()
    {
        // SameSite=Lax blocks sending the cookie cross-site, not setting a new one: login CSRF needs its own guard.
        var client = ApplicationFixture.CreateClientWithoutCsrfHeader();
        var user = ApplicationFixture.CreateUser();

        var response = await client.PostAsJsonAsync(
            $"{AuthenticationApi.BaseUrl}/login",
            new LoginPayload(user.Login, TestUserFactory.TestUserPassword));

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task IsLocked_WithoutTheHeader_ShouldBeAllowed()
    {
        var client = ApplicationFixture.CreateClientWithoutCsrfHeader();

        var response = await client.GetAsync($"{AuthenticationApi.BaseUrl}/is-locked");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    private async Task<HttpClient> CreateLoggedClientAsync(bool withCsrfHeader)
    {
        var client = withCsrfHeader
            ? ApplicationFixture.CreateClient()
            : ApplicationFixture.CreateClientWithoutCsrfHeader();
        await ApplicationFixture.AsLoggedAsync(client, ApplicationFixture.CreateUser());
        return client;
    }
}