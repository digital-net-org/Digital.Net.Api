using System.Net;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Endpoints.Authorization;

public class ApplicationAuthorizationTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    private const string ValidApplicationKey = "test-application-secret-key-for-integration-tests";

    [Test]
    public async Task Authorize_WithValidApplicationKey_ShouldReturnOk()
    {
        var client = ApplicationFixture.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApplicationKeyHeaderAccessor, ValidApplicationKey);
        var response = await client.TestApplicationAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnInvalidKey()
    {
        var client = ApplicationFixture.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApplicationKeyHeaderAccessor, "invalid-key");
        var response = await client.TestApplicationAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingHeader()
    {
        var response = await ApplicationFixture.CreateClient().TestApplicationAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}
