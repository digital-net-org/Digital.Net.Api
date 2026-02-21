using Digital.Net.Api.Core.Errors;
using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Core.Test.Errors;

public class TryCatchUtilitiesTests : UnitTest
{
    [Test]
    public async Task TryOrNull_ReturnsResult_WhenFunctionExecutesSuccessfully()
    {
        var result = TryCatchUtilities.TryOrNull<int>(() => 42);
        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task TryOrNull_ReturnsNull_WhenFunctionThrowsOnString()
    {
        var result = TryCatchUtilities.TryOrNull<string>(
            () => throw new InvalidOperationException());
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryOrNull_ReturnsNull_WhenFunctionThrowsOnInt()
    {
        var result = TryCatchUtilities.TryOrNull<int>(
            () => throw new InvalidOperationException());
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryOrNullAsync_ReturnsResult_WhenFunctionExecutesSuccessfully()
    {
        var result = await TryCatchUtilities.TryOrNullAsync<int>(async () => await Task.FromResult(42));
        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task TryOrNullAsync_ReturnsNull_WhenFunctionThrowsException()
    {
        var result = await TryCatchUtilities.TryOrNullAsync<string>(
            async () => throw new InvalidOperationException());
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryAll_ReturnsNull_WhenAllFunctionsReturnNull()
    {
        var result = TryCatchUtilities.TryAll<string>(
            () => null,
            () => null
        );
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryAll_ReturnsResult_WhenFirstFunctionExecutesSuccessfully()
    {
        var result = TryCatchUtilities.TryAll<int>(
            () => 42,
            () => throw new InvalidOperationException()
        );
        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task TryAll_ReturnsResult_WhenLastFunctionExecutesSuccessfully()
    {
        var result = TryCatchUtilities.TryAll<int>(
            () => throw new InvalidOperationException(),
            () => 42
        );
        await Assert.That(result).IsEqualTo(42);
    }
}