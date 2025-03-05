using System.Net;
using Digital.Core.Api.Test.Collections;
using Digital.Lib.Net.Authentication.Events;
using Digital.Lib.Net.Authentication.Options;
using Digital.Lib.Net.Core.Extensions.StringUtilities;
using Digital.Lib.Net.Entities.Models.Events;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Http.HttpClient.Extensions;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Authentication.JwtTests;

public class JwtLoginTest(AppFactory<Program> fixture) : AuthenticationTest(fixture)
{
    [Fact]
    public async Task Login_OnSuccess_ShouldReturnTokensAndGenerateEvents()
    {
        var user = GetUser();
        await ExecuteTest(
            user, 
            await BaseClient.Login(user), 
            EventState.Success, 
            HttpStatusCode.OK
        );
    }

    [Fact]
    public async Task Login_OnWrongPassword_ShouldReturnUnauthorized()
    {
        var user = GetUser();
        await ExecuteTest(
            user, 
            await BaseClient.Login(user.Login, "wrong password"), 
            EventState.Failed, 
            HttpStatusCode.Unauthorized
        );
    }

    
    [Fact]
    public async Task Login_OnInactiveUser_ShouldReturnUnauthorized()
    {
        var user = GetInactiveUser();
        await ExecuteTest(
            user, 
            await BaseClient.Login(user), 
            EventState.Failed, 
            HttpStatusCode.Unauthorized
        );
    }
    
    [Fact]
     public async Task Login_OnMaxAttempts_ShouldReturnTooManyRequests()
     {
         var user = GetUser();
         for (var i = 0; i < DefaultAuthenticationOptions.MaxLoginAttempts; i++)
             await BaseClient.Login(user.Login, "wrongPassword");

         await ExecuteTest(
             user, 
             await BaseClient.Login(user.Login, "wrongPassword"), 
             EventState.Failed, 
             HttpStatusCode.TooManyRequests
         );
     }

    private async Task ExecuteTest(
        User user,
        HttpResponseMessage result,
        EventState expectedState,
        HttpStatusCode expectedStatus
    )
    {
        var loginEvent = GetUserEvents(user).First();
        var storedToken = GetUserTokens(user).FirstOrDefault();
        var tokens = new List<string?>
        {
            result.Headers.TryGetCookie(CookieName),
            await result.Content.ReadAsStringAsync()
        };

        Assert.Equal(expectedStatus, result.StatusCode);
        Assert.True(loginEvent.Name == AuthenticationEvents.Login);
        Assert.True(loginEvent.State == expectedState);
        
        foreach (var token in tokens) 
            Assert.True(expectedState == EventState.Success 
                ? (token ?? string.Empty).IsJsonWebToken()
                : !(token ?? string.Empty).IsJsonWebToken()
            );
        
        if (expectedState == EventState.Success)
            Assert.NotNull(storedToken);
        else
            Assert.Null(storedToken);
    }
}
