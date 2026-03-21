using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core;

namespace Digital.Net.Lib.Test.Messages;

public class ResultMessageTest : UnitTest
{
    [Test]
    public async Task ConstructorTest_ReturnsExceptionValues_WhenCastedWithException()
    {
        var ex = new Exception("Something went wrong");
        var result = new ResultMessage(ex);
        await Assert.That(result.Message).IsEqualTo("Something went wrong");
        await Assert.That(result.Reference).IsEqualTo("SYSTEM_EXCEPTION");
        await Assert.That(result.StackTrace).IsNull();
        await Assert.That(result.Code).Matches(@"0x[0-9A-F]{8}");
    }

    [Test]
    public async Task Constructor_SetsMessage_WhenCreatedWithString()
    {
        var result = new ResultMessage("plain info message");
        await Assert.That(result.Message).IsEqualTo("plain info message");
        await Assert.That(result.Reference).IsEqualTo("UNREFERENCED_MESSAGE");
        await Assert.That(result.StackTrace).IsNull();
        await Assert.That(result.Code).IsNull();
    }

    [Test]
    public async Task Throw_ThrowsStoredException_WhenBuiltFromException()
    {
        var ex = new InvalidOperationException("boom");
        var result = new ResultMessage(ex);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            result.Throw();
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task Throw_ThrowsGenericException_WhenBuiltFromString()
    {
        var result = new ResultMessage("fallback message");
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            result.Throw();
            await Task.CompletedTask;
        });
    }
}
