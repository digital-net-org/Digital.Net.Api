using Digital.Net.Api.Core.Extensions.StringUtilities;

namespace Digital.Net.Api.Core.Test.Extensions.StringUtilities;

public class StringMatcherTest
{
    private const string ValidToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    private const string InvalidToken = "e.a";

    [Fact]
    public void IsJsonWebToken_ReturnsTrue_WhenValidToken()
    {
        var result = ValidToken.IsJsonWebToken();
        Assert.True(result);
    }

    [Fact]
    public void IsJsonWebToken_ReturnsFalse_WhenInvalidToken()
    {
        Assert.False(InvalidToken.IsJsonWebToken());
        Assert.False(StringMatcher.IsJsonWebToken(null));
        Assert.False(string.Empty.IsJsonWebToken());
    }
}