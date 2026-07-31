using System.Net;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Entities.Models.Sessions;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authorization;

public class SessionAuthorizationTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task LoggedUser_OnProtectedRoute_ShouldBeAuthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();

        await client.Login(user);
        var response = await client.TestSessionAuthorization();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task InactiveUser_OnLoginRoute_ShouldNotBeAuthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = false });
        var response = await client.Login(user);
        var loginEvent = await ApplicationFixture
            .GetContext().AuthEvents
            .Where(x => x.UserId == user.Id).OrderByDescending(x => x.CreatedAt)
            .FirstAsync();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(loginEvent.Type).EqualTo(AuthEventType.Login);
        await Assert.That(loginEvent.Success).IsFalse();
    }

    [Test]
    public async Task MissingCookie_ShouldNotBeAuthorized()
    {
        var response = await ApplicationFixture.CreateClient().TestSessionAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task UnknownCookieValue_ShouldNotBeAuthorized()
    {
        var client = ApplicationFixture.CreateClient();
        client.SetCookie(AuthenticationApi.CookieName, "not-a-known-session-id");

        var response = await client.TestSessionAuthorization();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task InactiveUser_OnProtectedRoute_ShouldNotBeAuthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true });
        await client.Login(user);

        var context = ApplicationFixture.GetContext();
        var userInDb = await context.Users.FindAsync(user.Id);
        userInDb!.IsActive = false;
        context.Users.Update(userInDb);
        await context.SaveChangesAsync();

        var response = await client.TestSessionAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task RevokedSession_ShouldBeRejectedImmediately()
    {
        // The whole point of the migration: a logout kills access on the very next request.
        var (client, sessionId) = await CreateLoggedClientAsync();
        await client.Logout();
        client.SetCookie(AuthenticationApi.CookieName, sessionId);

        var response = await client.TestSessionAuthorization();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    // Expiry rules — idle, absolute, opportunistic delete — are covered by SessionServiceTest, where they
    // belong. Repeating them over HTTP would only re-test the 401 that UnknownCookieValue already proves.

    [Test]
    public async Task RejectedCookie_ShouldBeClearedOnTheResponse()
    {
        var client = ApplicationFixture.CreateClient();
        client.SetCookie(AuthenticationApi.CookieName, "not-a-known-session-id");

        var response = await client.TestSessionAuthorization();

        var directive = response.TryGetSetCookie(AuthenticationApi.CookieName);
        await Assert.That(directive).IsNotNull();
        await Assert.That(directive!.ToLowerInvariant()).Contains("path=/");
    }

    private async Task<(HttpClient client, string sessionId)> CreateLoggedClientAsync()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        var sessionId = await ApplicationFixture.AsLoggedAsync(client, user);
        return (client, sessionId);
    }
}
