using System.Net;
using Digital.Net.Core;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authorization;

public class RefreshAuthorizationTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task Authorize_WithValidRefreshCookie_ShouldReturnOk()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await client.Login(user);
        // Clear the bearer the login also set: the Refresh-only route must authorize from the cookie alone.
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.TestRefreshAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingCookie()
    {
        var response = await ApplicationFixture.CreateClient().TestRefreshAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnInvalidCookie()
    {
        var client = ApplicationFixture.CreateClient();
        client.AddCookie($"{CookieName}=not-a-valid-jwt");
        var response = await client.TestRefreshAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    private string CookieName =>
        $"{ApplicationFixture.GetConfiguration<string>(CoreSettings.DomainKey) ?? throw new Exception()}_refresh";
}
