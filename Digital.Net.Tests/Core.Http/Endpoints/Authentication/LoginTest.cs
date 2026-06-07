using System.Net;
using Digital.Net.Core;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Options;
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
    public async Task Login_OnSuccess_ShouldReturnTokensAndRecordAuthEvent()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await ExecuteTestAsync(user, await client.Login(user), true, HttpStatusCode.OK);
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
    public async Task Login_OnMaxCurrentSessions_ShouldInvalidateOldestSession()
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

        await Assert.That(successCount).EqualTo(maxSessions + 1);

        var failure = await clients.First().RefreshTokens();
        await Assert.That(failure.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    private string CookieName =>
        $"{ApplicationFixture.GetConfiguration<string>(CoreSettings.DomainKey) ?? throw new Exception()}_refresh";

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
        var storedToken = ApplicationFixture
            .GetContext().ApiTokens
            .FirstOrDefault(x => x.UserId == user.Id);

        var tokens = new List<string?>
        {
            result.TryGetCookie()?.Split(';').FirstOrDefault()?.Split($"{CookieName}=")[1],
            await result.Content.ReadAsStringAsync()
        };

        await Assert.That(result.StatusCode).EqualTo(expectedStatus);
        await Assert.That(loginEvent.Type == AuthEventType.Login).IsTrue();
        await Assert.That(loginEvent.Success == expectedSuccess).IsTrue();

        foreach (var token in tokens)
            await Assert.That(expectedSuccess
                ? IsJsonWebToken(token ?? string.Empty)
                : !IsJsonWebToken(token ?? string.Empty)
            ).IsTrue();

        if (expectedSuccess)
            await Assert.That(storedToken).IsNotNull();
        else
            await Assert.That(storedToken).IsNull();
    }

    private static bool IsJsonWebToken(string? token = null) =>
        !string.IsNullOrWhiteSpace(token) && token.Split('.').Length == 3;
}
