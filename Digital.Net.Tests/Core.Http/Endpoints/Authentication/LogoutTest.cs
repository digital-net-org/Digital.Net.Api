using System.Net;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authentication;

public class LogoutTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task Logout_ShouldLogoutClient()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await client.Login(user);

        var result = await client.Logout();
        await ExecuteTestAsync(result, user, AuthEventType.Logout);
    }

    [Test]
    public async Task Logout_ShouldClearTheSessionCookie()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await client.Login(user);

        var result = await client.Logout();

        // Same attributes as the creation directive, otherwise the browser keeps a cookie we believe is gone.
        var directive = result.TryGetSetCookie(AuthenticationApi.CookieName)!.ToLowerInvariant();
        await Assert.That(directive).Contains("path=/");
        await Assert.That(directive).DoesNotContain("domain=");
    }

    [Test]
    public async Task LogoutAll_ShouldLogoutAllClients()
    {
        var client = ApplicationFixture.CreateClient();
        var secondClient = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await client.Login(user);
        await secondClient.Login(user);

        var result = await client.LogoutAll();

        // The other device must be locked out too — counting rows alone would not prove it.
        var otherDevice = await secondClient.TestSessionAuthorization();
        await Assert.That(otherDevice.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
        await ExecuteTestAsync(result, user, AuthEventType.LogoutAll);
    }

    [Test]
    public async Task LogoutAll_WithApiKey_ShouldRevokeAllSessions()
    {
        // The route advertises Session|ApiKey; resolving the user from the cookie used to make ApiKey unusable.
        var user = ApplicationFixture.CreateUser();
        var browser = ApplicationFixture.CreateClient();
        await browser.Login(user);

        var apiKey = Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128);
        var context = ApplicationFixture.GetContext();
        await context.ApiKeys.AddAsync(new ApiKey(user.Id, "logout-all-key", apiKey));
        await context.SaveChangesAsync();

        var apiKeyClient = ApplicationFixture.CreateClient();
        apiKeyClient.DefaultRequestHeaders.Add(AuthenticationStaticOptions.ApiKeyHeaderAccessor, apiKey);

        var result = await apiKeyClient.LogoutAll();

        await Assert.That(result.StatusCode).EqualTo(HttpStatusCode.NoContent);
        await Assert.That((await browser.TestSessionAuthorization()).StatusCode)
            .EqualTo(HttpStatusCode.Unauthorized);
    }

    private async Task ExecuteTestAsync(
        HttpResponseMessage result,
        User user,
        AuthEventType eventType
    )
    {
        var logoutEvent = ApplicationFixture
            .GetContext().AuthEvents
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .First();
        var userSessions = ApplicationFixture
            .GetContext().Sessions
            .Where(x => x.UserId == user.Id)
            .ToList();

        await Assert.That(result.StatusCode).EqualTo(HttpStatusCode.NoContent);
        await Assert.That(logoutEvent.Type).EqualTo(eventType);
        await Assert.That(logoutEvent.Success).IsTrue();
        await Assert.That(userSessions).IsEmpty();
    }
}
