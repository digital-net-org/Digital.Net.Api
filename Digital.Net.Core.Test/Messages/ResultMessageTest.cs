using Digital.Net.Core.Messages;
using Digital.Net.Tests.Core;

namespace Digital.Net.Core.Test.Messages;

public class ResultMessageTest : UnitTest
{
    [Test]
    public async Task ConstructorTest_ReturnsExceptionValues_WhenCastedWithException()
    {
        var ex = new Exception("Something went wrong");
        var result = new ResultMessage(ex);
        await Assert.That(result.Message).IsEqualTo("Something went wrong");
        await Assert.That(result.Reference).IsEqualTo("SYSTEM_EXCEPTION");
        await Assert.That(result.StackTrace).IsEqualTo(ex.StackTrace);
        await Assert.That(result.Code).Matches(@"0x[0-9A-F]{8}");
    }
}
