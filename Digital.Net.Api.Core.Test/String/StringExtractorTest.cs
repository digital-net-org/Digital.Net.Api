using Digital.Net.Api.Core.String;
using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Core.Test.String;

public class StringExtractorTest : UnitTest
{
    [Test]
    public async Task ExtractFromPath_ReturnsParts_WhenPathIsFormatted()
    {
        const string path = "/Something/like/this";
        var result = path.ExtractFromPath();
        await Assert.That(result.Count).IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo("Something");
        await Assert.That(result[1]).IsEqualTo("like");
        await Assert.That(result[2]).IsEqualTo("this");
    }

    [Test]
    public async Task ExtractFromPath_ReturnsParts_WhenOnlyOneFolder()
    {
        const string path = "/Something";
        var result = path.ExtractFromPath();
        await Assert.That(result).HasSingleItem();
        await Assert.That(result[0]).IsEqualTo("Something");
    }

    [Test]
    public async Task ExtractFromPath_ReturnsParts_WhenPathIsFormattedWithBackslashes()
    {
        const string path = "Like\\That";
        var result = path.ExtractFromPath();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result[0]).IsEqualTo("Like");
        await Assert.That(result[1]).IsEqualTo("That");
    }
}