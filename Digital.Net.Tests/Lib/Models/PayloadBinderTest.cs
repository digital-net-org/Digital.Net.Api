using Digital.Net.Lib.Models;
using Digital.Net.Tests.Core;

namespace Digital.Net.Tests.Lib.Models;

public class PayloadBinderTests : UnitTest
{
    [Test]
    public async Task Bind_CopiesMatchingProperties()
    {
        var result = PayloadBinder.Bind<Source, Target>(new Source { Value = 42, Name = "Test" });
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<Target>();
        await Assert.That(result.Value).IsEqualTo(42);
    }

    [Test]
    public async Task Bind_LeavesDefault_WhenNoMatchingProperty()
    {
        var result = PayloadBinder.Bind<Source, TargetNoMatch>(new Source { Value = 42 });
        await Assert.That(result).IsNotNull();
        await Assert.That(result.NotANumber).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Bind_SkipsProperty_WhenTypeDiffers()
    {
        // Source.Value is int, target Value is string -> no name+type match -> left at default (null).
        var result = PayloadBinder.Bind<Source, TargetTypeMismatch>(new Source { Value = 42 });
        await Assert.That(result.Value).IsNull();
    }

    private class Source
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class Target
    {
        public int Value { get; set; }
    }

    private class TargetNoMatch
    {
        public readonly string NotANumber = string.Empty;
    }

    private class TargetTypeMismatch
    {
        public string? Value { get; set; }
    }
}
