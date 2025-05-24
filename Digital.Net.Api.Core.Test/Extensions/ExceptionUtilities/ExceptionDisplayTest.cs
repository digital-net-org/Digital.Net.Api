using Digital.Net.Api.Core.Extensions.ExceptionUtilities;
using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Core.Test.Extensions.ExceptionUtilities;

public class ExceptionDisplayTest : UnitTest
{
    [Fact]
    public void GetReferenceTest()
    {
        var ex = new Exception();
        var result = ex.GetReference();
        Assert.Equal("SYSTEM_EXCEPTION", result);
    }

    [Fact]
    public void GetFormattedErrorCodeTest() =>
        Assert.Equal("0x80131500", new Exception().GetFormattedErrorCode());

    [Fact]
    public void GetFormattedErrorCodeTest_WithCustomErrorCode() =>
        Assert.Equal("0x80070057", new ArgumentException("Something went wrong").GetFormattedErrorCode());
}