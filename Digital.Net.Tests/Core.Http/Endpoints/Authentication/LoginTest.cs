using System.Net;
using Digital.Net.Core;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Entities.Models.Sessions;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authentication;

public class LoginTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task Login_OnSuccess_ShouldOpenASessionAndRecordAuthEvent()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await ExecuteTestAsync(user, await client.Login(user), true, HttpStatusCode.OK);
    }

    [Test]
    public async Task Login_OnSuccess_ShouldSetAHardenedSessionCookie()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();

        var response = await client.Login(user);
        var directive = response.TryGetSetCookie(AuthenticationApi.CookieName)!.ToLowerInvariant();

        await Assert.That(directive).Contains("httponly");
        await Assert.That(directive).Contains("secure");
        // Lax because the fixture declares a single allowed origin, hence a single site.
        await Assert.That(directive).Contains("samesite=lax");
        await Assert.That(directive).Contains("path=/");
        await Assert.That(directive).Contains("expires=");
        // Host-only on purpose: a Domain attribute would leak the cookie to every sibling subdomain.
        await Assert.That(directive).DoesNotContain("domain=");
    }

    [Test]
    public async Task Login_WithAnOriginOutsideTheApplicationDomain_ShouldRelaxSameSiteToNone()
    {
        // A cross-site client would never send back a Lax cookie, whatever CORS allows. The
        // DN-Requested-With check stays the CSRF barrier in both cases.
        await using var factory = new ApplicationFactory(
            ApplicationFixture.Fixture.ConnectionString,
            new Dictionary<string, string?>
            {
                [$"{CoreSettings.CorsAllowedOriginsKey}:0"] = ApplicationFactory.TestOrigin,
                [$"{CoreSettings.CorsAllowedOriginsKey}:1"] = "https://console.other-domain.test"
            }
        );
        // Same database, but through the fixture's dedicated context: the factory's scoped one is shared.
        var user = ApplicationFixture.CreateUser();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.CsrfHeaderAccessor, "digital-net");
        client.DefaultRequestHeaders.Add(TestRemoteIpStartupFilter.Header, "10.99.99.99");

        var response = await client.Login(user);
        var directive = response.TryGetSetCookie(AuthenticationApi.CookieName)!.ToLowerInvariant();

        await Assert.That(directive).Contains("samesite=none");
        await Assert.That(directive).Contains("secure");
    }

    [Test]
    public async Task Login_OnSuccess_ShouldNeverReturnTheSessionId()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();

        var response = await client.Login(user);
        var sessionId = response.TryGetCookieValue(AuthenticationApi.CookieName)!;
        var body = await response.Content.ReadAsStringAsync();
        var result = await response.Content.ReadContentAsync<Result>();

        // The cookie is the only carrier. Nothing usable by JavaScript is handed back — that is the
        // point of dropping localStorage — and the identity still comes from user/self.
        await Assert.That(body).DoesNotContain(sessionId);
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task Login_OnSuccess_ShouldStoreOnlyTheHashedSessionId()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();

        var response = await client.Login(user);
        var sessionId = response.TryGetCookieValue(AuthenticationApi.CookieName)!;

        var stored = await ApplicationFixture.GetContext().Sessions.FirstAsync(s => s.UserId == user.Id);
        await Assert.That(stored.Key).IsEqualTo(Session.Hash(sessionId));
        await Assert.That(stored.Key).IsNotEqualTo(sessionId);
    }

    [Test]
    public async Task Login_OnWrongPassword_ShouldReturnUnauthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await ExecuteTestAsync(
            user,
            await client.Login(user.Login, "wrong password"),
            false,
            HttpStatusCode.Unauthorized
        );
    }

    [Test]
    public async Task Login_OnInactiveUser_ShouldReturnUnauthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = false });
        await ExecuteTestAsync(user, await client.Login(user), false, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_WithoutIpAddress_ShouldReturnUnauthorized()
    {
        var client = ApplicationFixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Add(AuthenticationStaticOptions.CsrfHeaderAccessor, "digital-net");
        var user = ApplicationFixture.CreateUser();
        var response = await client.Login(user);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_OnMaxAttempts_ShouldReturnTooManyRequests()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        for (var i = 0; i < AuthenticationStaticOptions.MaxLoginAttempts; i++)
            await client.Login(user.Login, "wrongPassword");

        await ExecuteTestAsync(
            user,
            await client.Login(user.Login, "wrongPassword"),
            false,
            HttpStatusCode.TooManyRequests
        );
    }

    [Test]
    public async Task Login_OnMaxAccountAttempts_ShouldLockAccountEvenFromAFreshIp()
    {
        var user = ApplicationFixture.CreateUser();
        for (var i = 0; i < AuthenticationStaticOptions.MaxAccountLoginAttempts; i++)
            await ApplicationFixture.CreateClient().Login(user.Login, "wrongPassword");

        var response = await ApplicationFixture.CreateClient().Login(user.Login, "wrongPassword");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task Login_OnMaxCurrentSessions_ShouldEvictTheSurplusSession()
    {
        const int maxSessions = AuthenticationStaticOptions.MaxConcurrentSessions;
        var clients = new List<HttpClient>();
        var user = ApplicationFixture.CreateUser();

        for (var i = 0; i < maxSessions + 1; i++)
        {
            var c = ApplicationFixture.CreateClient();
            clients.Add(c);
            await c.Login(user);
        }

        var successCount = await ApplicationFixture
            .GetContext().AuthEvents
            .CountAsync(e => e.UserId == user.Id && e.Type == AuthEventType.Login && e.Success);
        var storedSessions = await ApplicationFixture.GetContext().Sessions.CountAsync(s => s.UserId == user.Id);

        await Assert.That(successCount).EqualTo(maxSessions + 1);
        await Assert.That(storedSessions).EqualTo(maxSessions);

        var evicted = await clients.First().TestSessionAuthorization();
        await Assert.That(evicted.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    private async Task ExecuteTestAsync(
        User user,
        HttpResponseMessage result,
        bool expectedSuccess,
        HttpStatusCode expectedStatus
    )
    {
        var loginEvent = ApplicationFixture
            .GetContext().AuthEvents
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .First();
        var storedSession = ApplicationFixture
            .GetContext().Sessions
            .FirstOrDefault(x => x.UserId == user.Id);
        var sessionCookie = result.TryGetCookieValue(AuthenticationApi.CookieName);

        await Assert.That(result.StatusCode).EqualTo(expectedStatus);
        await Assert.That(loginEvent.Type).EqualTo(AuthEventType.Login);
        await Assert.That(loginEvent.Success).IsEqualTo(expectedSuccess);

        if (expectedSuccess)
        {
            await Assert.That(storedSession).IsNotNull();
            await Assert.That(sessionCookie).IsNotNull();
            await Assert.That(sessionCookie!.Length).IsEqualTo(AuthenticationStaticOptions.SessionIdLength);
        }
        else
        {
            await Assert.That(storedSession).IsNull();
            await Assert.That(sessionCookie).IsNull();
        }
    }
}
