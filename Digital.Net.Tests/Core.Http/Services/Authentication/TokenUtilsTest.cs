using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Digital.Net.Core.Http.Services.Authentication.Exceptions;
using Digital.Net.Core.Http.Services.Authentication.Utils;

namespace Digital.Net.Tests.Core.Http.Services.Authentication;

public class TokenUtilsTest : UnitTest
{
    /// <summary>
    ///     Builds an (unsigned) token with a controlled <c>iat</c>/<c>exp</c> so the renewal window can be
    ///     exercised without going through signing or the handler's lifetime validation.
    /// </summary>
    private static JwtSecurityToken BuildToken(TimeSpan issuedAgo, TimeSpan expiresIn)
    {
        var now = DateTime.UtcNow;
        return new JwtSecurityToken(
            claims:
            [
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(now - issuedAgo, TimeSpan.Zero).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            ],
            expires: now + expiresIn);
    }

    [Test]
    public async Task ShouldRenewCookie_ReturnsTrue_WhenMoreThanOneDayLeft()
    {
        var token = BuildToken(issuedAgo: TimeSpan.FromMinutes(1), expiresIn: TimeSpan.FromDays(2));
        await Assert.That(token.ShouldRenewCookie()).IsTrue();
    }

    [Test]
    public async Task ShouldRenewCookie_ReturnsTrue_WhenLessThanTwentyPercentLeft()
    {
        // 110 min lifetime, 10 min left -> ~9% remaining, well under the 20% threshold.
        var token = BuildToken(issuedAgo: TimeSpan.FromMinutes(100), expiresIn: TimeSpan.FromMinutes(10));
        await Assert.That(token.ShouldRenewCookie()).IsTrue();
    }

    [Test]
    public async Task ShouldRenewCookie_ReturnsFalse_WhenInsideMiddleBand()
    {
        // 120 min lifetime, 60 min left -> 50% remaining and under a day: no renewal.
        var token = BuildToken(issuedAgo: TimeSpan.FromMinutes(60), expiresIn: TimeSpan.FromMinutes(60));
        await Assert.That(token.ShouldRenewCookie()).IsFalse();
    }

    [Test]
    public async Task ShouldRenewCookie_Throws_WhenTokenExpired()
    {
        var token = BuildToken(issuedAgo: TimeSpan.FromMinutes(60), expiresIn: TimeSpan.FromMinutes(-1));
        await Assert.ThrowsAsync<InvalidTokenException>(async () =>
        {
            token.ShouldRenewCookie();
            await Task.CompletedTask;
        });
    }
}
