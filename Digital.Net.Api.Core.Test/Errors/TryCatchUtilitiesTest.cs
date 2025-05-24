using Digital.Net.Api.Core.Errors;
using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Core.Test.Errors;

public class TryCatchUtilitiesTests : UnitTest
{
    [Fact]
    public void TryOrNull_ReturnsResult_WhenFunctionExecutesSuccessfully()
    {
        var result = TryCatchUtilities.TryOrNull<int>(() => 42);
        Assert.Equal(42, result);
    }

    [Fact]
    public void TryOrNull_ReturnsNull_WhenFunctionThrowsOnString()
    {
        var result = TryCatchUtilities.TryOrNull<string>(
            () => throw new InvalidOperationException());
        Assert.Null(result);
    }

    [Fact]
    public void TryOrNull_ReturnsNull_WhenFunctionThrowsOnInt()
    {
        var result = TryCatchUtilities.TryOrNull<int>(
            () => throw new InvalidOperationException());
        Assert.Null(result);
    }

    [Fact]
    public async Task TryOrNullAsync_ReturnsResult_WhenFunctionExecutesSuccessfully()
    {
        var result = await TryCatchUtilities.TryOrNullAsync<int>(async () => await Task.FromResult(42));
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task TryOrNullAsync_ReturnsNull_WhenFunctionThrowsException()
    {
        var result = await TryCatchUtilities.TryOrNullAsync<string>(
            async () => throw new InvalidOperationException());
        Assert.Null(result);
    }

    [Fact]
    public void TryAll_ReturnsNull_WhenAllFunctionsReturnNull()
    {
        var result = TryCatchUtilities.TryAll<string>(
            () => null,
            () => null
        );
        Assert.Null(result);
    }

    [Fact]
    public void TryAll_ReturnsResult_WhenFirstFunctionExecutesSuccessfully()
    {
        var result = TryCatchUtilities.TryAll<int>(
            () => 42,
            () => throw new InvalidOperationException()
        );
        Assert.Equal(42, result);
    }

    [Fact]
    public void TryAll_ReturnsResult_WhenLastFunctionExecutesSuccessfully()
    {
        var result = TryCatchUtilities.TryAll<int>(
            () => throw new InvalidOperationException(),
            () => 42
        );
        Assert.Equal(42, result);
    }
}