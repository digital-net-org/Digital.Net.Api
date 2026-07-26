using System.Net;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Http;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Endpoints.Authentication;

public class RefreshTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }
    
    [Test]
    public async Task RefreshTokens_WithValidRefreshToken_ShouldReturnToken()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await client.Login(user);
        var response = await client.RefreshTokens();
        var userTokens = ApplicationFixture.GetContext().ApiTokens.Where(x => x.UserId == user.Id).ToList();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(userTokens).HasSingleItem();
    }

    [Test]
    public async Task RefreshTokens_WithAlreadyRotatedToken_ShouldReturnUnauthorized()
    {
        // L1: a refresh token is single-use — replaying the pre-rotation one must fail.
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        var loginResponse = await client.Login(user);
        var initialCookie = loginResponse.TryGetCookie();

        await Assert.That((await client.RefreshTokens()).StatusCode).EqualTo(HttpStatusCode.OK);

        var replayClient = ApplicationFixture.CreateClient();
        replayClient.AddCookie(initialCookie!);
        var response = await replayClient.RefreshTokens();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task RefreshTokens_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await client.Login(user);
        await client.Logout();
        var response = await client.RefreshTokens();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}