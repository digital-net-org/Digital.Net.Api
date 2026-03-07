using Digital.Net.Core.Exceptions;
using Digital.Net.Tests.Core;

namespace Digital.Net.Core.Test.Exceptions;

public class ExceptionDisplayTest : UnitTest
{
    [Test]
    public async Task GetReferenceTest()
    {
        var ex = new Exception();
        var result = ex.GetReference();
        await Assert.That(result).IsEqualTo("SYSTEM_EXCEPTION");
    }

    [Test]
    public async Task GetFormattedErrorCodeTest() =>
        await Assert.That(new Exception().GetFormattedErrorCode()).IsEqualTo("0x80131500");

    [Test]
    public async Task GetFormattedErrorCodeTest_WithCustomErrorCode() =>
        await Assert.That(new ArgumentException("Something went wrong").GetFormattedErrorCode()).IsEqualTo("0x80070057");
}