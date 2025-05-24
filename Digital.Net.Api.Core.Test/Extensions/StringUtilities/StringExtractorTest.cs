using Digital.Net.Api.Core.Extensions.StringUtilities;
using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Core.Test.Extensions.StringUtilities;

public class StringExtractorTest : UnitTest
{
    [Fact]
    public void ExtractFromPath_ReturnsParts_WhenPathIsFormatted()
    {
        const string path = "/Something/like/this";
        var result = path.ExtractFromPath();
        Assert.Equal(3, result.Count);
        Assert.Equal("Something", result[0]);
        Assert.Equal("like", result[1]);
        Assert.Equal("this", result[2]);
    }

    [Fact]
    public void ExtractFromPath_ReturnsParts_WhenOnlyOneFolder()
    {
        const string path = "/Something";
        var result = path.ExtractFromPath();
        Assert.Single(result);
        Assert.Equal("Something", result[0]);
    }

    [Fact]
    public void ExtractFromPath_ReturnsParts_WhenPathIsFormattedWithBackslashes()
    {
        const string path = "Like\\That";
        var result = path.ExtractFromPath();
        Assert.Equal(2, result.Count);
        Assert.Equal("Like", result[0]);
        Assert.Equal("That", result[1]);
    }
}