using Digital.Net.Core.String;
using Digital.Net.Tests.Core;
using StringMatcher = Digital.Net.Core.String.StringMatcher;

namespace Digital.Net.Core.Test.String;

public class StringMatcherTest : UnitTest
{
    private const string ValidToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    private const string InvalidToken = "e.a";

    [Test]
    public async Task IsJsonWebToken_ReturnsTrue_WhenValidToken()
    {
        var result = ValidToken.IsJsonWebToken();
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsJsonWebToken_ReturnsFalse_WhenInvalidToken()
    {
        await Assert.That(InvalidToken.IsJsonWebToken()).IsFalse();
        await Assert.That(StringMatcher.IsJsonWebToken(null)).IsFalse();
        await Assert.That(string.Empty.IsJsonWebToken()).IsFalse();
    }
}