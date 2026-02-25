using System.Net;
using Digital.Net.Api.Authentication.Events;
using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Core.String;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Services.HttpContext.Extensions;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Test.Endpoints.JwtTests;

public class JwtLoginTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }
    
    [Test]
    public async Task Login_OnSuccess_ShouldReturnTokensAndGenerateEvents()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
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
        var client = Application.CreateClient();
        var user = Application.CreateUser();
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
        var client = Application.CreateClient();
        var user = Application.CreateUser(new TestUserPayload { IsActive = false });
        await ExecuteTestAsync(
            user,
            await client.Login(user),
            EventState.Failed,
            HttpStatusCode.Unauthorized
        );
    }

    [Test]
    public async Task Login_OnMaxAttempts_ShouldReturnTooManyRequests()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
        for (var i = 0; i < AuthenticationStaticOptions.MaxLoginAttempts; i++)
            await client.Login(user.Login, "wrongPassword");

        await ExecuteTestAsync(
            user,
            await client.Login(user.Login, "wrongPassword"),
            EventState.Failed,
            HttpStatusCode.TooManyRequests
        );
    }

    private string CookieName =>
        $"{Application.GetConfiguration<string>(AppSettings.DomainKey) ?? throw new Exception()}_refresh";

    private async Task ExecuteTestAsync(
        User user,
        HttpResponseMessage result,
        EventState expectedState,
        HttpStatusCode expectedStatus
    )
    {
        var loginEvent = Application
            .GetRepository<Event>()
            .Get(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .First();
        var storedToken = Application
            .GetRepository<ApiToken>()
            .Get(x => x.UserId == user.Id)
            .FirstOrDefault();
        
        var tokens = new List<string?>
        {
            result.Headers.TryGetCookie(CookieName),
            await result.Content.ReadAsStringAsync()
        };

        await Assert.That(result.StatusCode).EqualTo(expectedStatus);
        await Assert.That(loginEvent.Name == AuthenticationEvents.Login).IsTrue();
        await Assert.That(loginEvent.State == expectedState).IsTrue();

        foreach (var token in tokens)
            await Assert.That(expectedState == EventState.Success
                ? (token ?? string.Empty).IsJsonWebToken()
                : !(token ?? string.Empty).IsJsonWebToken()
            ).IsTrue();

        if (expectedState == EventState.Success)
            await Assert.That(storedToken).IsNotNull();
        else
            await Assert.That(storedToken).IsNull();
    }
}