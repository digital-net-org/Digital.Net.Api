using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core;

namespace Digital.Net.Tests.Lib.Messages;

public class ResultMessageTest : UnitTest
{
    [Test]
    public async Task Constructor_MasksMessageButKeepsMetadata_ForUntypedException()
    {
        // Outside Development (tests run as "Test"): an untyped exception's raw message must not leak.
        var ex = new Exception("Something went wrong");
        var result = new ResultMessage(ex);
        await Assert.That(result.Message).IsEqualTo("An unexpected error occurred");
        await Assert.That(result.Reference).IsEqualTo("SYSTEM_EXCEPTION");
        await Assert.That(result.StackTrace).IsNull();
        await Assert.That(result.Code).Matches(@"0x[0-9A-F]{8}");
    }

    [Test]
    public async Task Constructor_KeepsMessage_ForTypedBusinessException()
    {
        var result = new ResultMessage(new ResourceNotFoundException());
        await Assert.That(result.Message).IsEqualTo("Could not find resource");
    }

    [Test]
    public async Task Constructor_KeepsExplicitMessage_WhenProvided()
    {
        var result = new ResultMessage(new Exception("raw db error"), "Friendly message");
        await Assert.That(result.Message).IsEqualTo("Friendly message");
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
