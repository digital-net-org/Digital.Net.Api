using System.Net;
using Digital.Net.Api.Services.Authentication.Options;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Test.Endpoints.Authentication.ApplicationTests;

public class ApplicationAuthorizationTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    private const string ValidApplicationKey = "test-application-secret-key-for-integration-tests";

    [Test]
    public async Task Authorize_WithValidApplicationKey_ShouldReturnOk()
    {
        var client = Application.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApplicationKeyHeaderAccessor, ValidApplicationKey);
        var response = await client.TestApplicationAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnInvalidKey()
    {
        var client = Application.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApplicationKeyHeaderAccessor, "invalid-key");
        var response = await client.TestApplicationAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingHeader()
    {
        var response = await Application.CreateClient().TestApplicationAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}
