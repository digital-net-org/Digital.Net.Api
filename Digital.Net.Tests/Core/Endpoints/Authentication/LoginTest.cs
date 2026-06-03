using System.Net;
using Digital.Net.Core;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Services.Authentication.Events;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Http;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Core.Endpoints.Authentication;

public class LoginTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }
    
    [Test]
    public async Task Login_OnSuccess_ShouldReturnTokensAndGenerateEvents()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await ExecuteTestAsync(
            user,
            await client.Login(user),
            EventState.Success,
            HttpStatusCode.OK
        );
    }

    [Test]
    public async Task Login_OnWrongPassword_ShouldReturnUnauthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await ExecuteTestAsync(
            user,
            await client.Login(user.Login, "wrong password"),
            EventState.Failed,
            HttpStatusCode.Unauthorized
        );
    }


    [Test]
    public async Task Login_OnInactiveUser_ShouldReturnUnauthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = false });
        await ExecuteTestAsync(
            user,
            await client.Login(user),
            EventState.Failed,
            HttpStatusCode.Unauthorized
        );
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
            EventState.Failed,
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
            .GetContext().Events
            .CountAsync(e => e.UserId == user.Id
                             && e.Name == AuthenticationEvents.Login
                             && e.State == EventState.Success
            );

        await Assert.That(successCount).EqualTo(maxSessions + 1);

        var failure = await clients.First().RefreshTokens();
        await Assert.That(failure.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    private string CookieName =>
        $"{ApplicationFixture.GetConfiguration<string>(CoreSettings.DomainKey) ?? throw new Exception()}_refresh";

    private async Task ExecuteTestAsync(
        User user,
        HttpResponseMessage result,
        EventState expectedState,
        HttpStatusCode expectedStatus
    )
    {
        var loginEvent = ApplicationFixture
            .GetContext().Events
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
        await Assert.That(loginEvent.Name == AuthenticationEvents.Login).IsTrue();
        await Assert.That(loginEvent.State == expectedState).IsTrue();

        foreach (var token in tokens)
            await Assert.That(expectedState == EventState.Success
                ? IsJsonWebToken(token ?? string.Empty)
                : !IsJsonWebToken(token ?? string.Empty)
            ).IsTrue();

        if (expectedState == EventState.Success)
            await Assert.That(storedToken).IsNotNull();
        else
            await Assert.That(storedToken).IsNull();
    }

    private static bool IsJsonWebToken(string? token = null) =>
        !string.IsNullOrWhiteSpace(token) && token.Split('.').Length == 3;
}