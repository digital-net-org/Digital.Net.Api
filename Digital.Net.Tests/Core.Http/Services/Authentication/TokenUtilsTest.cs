using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Http.Services.Authentication.Types;
using Digital.Net.Core.Http.Services.Authentication.Utils;

namespace Digital.Net.Tests.Core.Http.Services.Authentication;

public class TokenUtilsTest : UnitTest
{
    private static JwtSecurityToken BuildToken(params Claim[] claims) =>
        new(claims: claims, expires: DateTime.UtcNow.AddMinutes(5));

    [Test]
    [Arguments(TokenType.Access)]
    [Arguments(TokenType.Refresh)]
    public async Task GetTokenType_ReturnsType_WhenClaimIsPresent(TokenType type)
    {
        var token = BuildToken(new Claim(AuthenticationStaticOptions.TokenTypeClaimType, type.ToString()));
        await Assert.That(token.GetTokenType()).IsEqualTo(type);
    }

    [Test]
    public async Task GetTokenType_ReturnsNull_WhenClaimIsMissing()
    {
        var token = BuildToken(new Claim(JwtRegisteredClaimNames.Sub, "someone"));
        await Assert.That(token.GetTokenType()).IsNull();
    }

    [Test]
    public async Task GetTokenType_ReturnsNull_WhenClaimIsNotAKnownType()
    {
        var token = BuildToken(new Claim(AuthenticationStaticOptions.TokenTypeClaimType, "Whatever"));
        await Assert.That(token.GetTokenType()).IsNull();
    }
}
