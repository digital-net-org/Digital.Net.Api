using System.Net;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Tests.Endpoints;

public class JwtRefreshTest : AuthenticationTest
{
    [Test]
    public async Task RefreshTokens_WithValidRefreshToken_ShouldReturnToken()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
        await client.Login(user);
        var response = await client.RefreshTokens();
        var userTokens = GetUserTokens(user).ToList();
        // var cookieToken = response.Headers.TryGetCookie(CookieName);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        // Assert.Equal(cookieToken, userTokens.First().Key);
        await Assert.That(userTokens).HasSingleItem();

        // TODO: Find a way to test the 80% consumption rule of the refresh token
        // foreach (var token in new List<string?> { cookieToken, await response.Content.ReadAsStringAsync() })
        //     Assert.True(token!.IsJsonWebToken());
    }

    [Test]
    public async Task RefreshTokens_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
        await client.Login(user);
        await client.Logout();
        var response = await client.RefreshTokens();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}