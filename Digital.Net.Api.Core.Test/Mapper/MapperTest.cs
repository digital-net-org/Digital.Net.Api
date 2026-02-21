using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Core.Test.Mapper;

public class MapperTests : UnitTest
{
    [Test]
    public async Task Map_ReturnsInstanceOfTargetType_WhenMatchingPropertiesFound()
    {
        var source = new Source { Value = 42 };
        var result = Models.Mapper.Map<Source, Target>(source);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<Target>();
        await Assert.That(result.Value).IsEqualTo(42);
    }

    [Test]
    public async Task Map_ReturnsInstanceOfTargetType_WhenMatchingPropertiesNotFound()
    {
        var source = new Source { Value = 42 };
        var result = Models.Mapper.Map<Source, TargetFail>(source);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<TargetFail>();
        await Assert.That(result.NotANumber).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Map_ReturnsListOfTargetType_WhenMatchingPropertiesFound()
    {
        var source = new List<Source>
        {
            new() { Value = 42 },
            new() { Value = 43 },
            new() { Value = 44 }
        };
        var result = Models.Mapper.Map<Source, Target>(source);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<List<Target>>();
        await Assert.That(result.Count).IsEqualTo(3);
        await Assert.That(result[0].Value).IsEqualTo(42);
        await Assert.That(result[1].Value).IsEqualTo(43);
        await Assert.That(result[2].Value).IsEqualTo(44);
    }

    [Test]
    public async Task MapFromConstructor_ReturnsInstanceOfTargetType_WhenSuitableConstructorFound()
    {
        var source = new FromConstructorSource { Value = 42 };
        var result = Models.Mapper.MapFromConstructor<FromConstructorSource, FromConstructorTarget>(source);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<FromConstructorTarget>();
        await Assert.That(result.Value).IsEqualTo(42);
    }

    [Test]
    public async Task MapFromConstructor_ThrowsInvalidOperationException_WhenNoSuitableConstructorFound()
    {
        var source = new FromConstructorSource { Value = 42 };
        await Assert.That(() => Models.Mapper.MapFromConstructor<FromConstructorSource, TestNoSuitableConstructorClass>(source))
            .ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task MapFromConstructor_ReturnsListOfTargetType_WhenSuitableConstructorFound()
    {
        var source = new List<FromConstructorSource>
        {
            new() { Value = 42 },
            new() { Value = 43 },
            new() { Value = 44 }
        };
        var result = Models.Mapper.MapFromConstructor<FromConstructorSource, FromConstructorTarget>(source);
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<List<FromConstructorTarget>>();
        await Assert.That(result.Count).IsEqualTo(3);
        await Assert.That(result[0].Value).IsEqualTo(42);
        await Assert.That(result[1].Value).IsEqualTo(43);
        await Assert.That(result[2].Value).IsEqualTo(44);
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

    private class TargetFail
    {
        public readonly string NotANumber = string.Empty;
    }

    private class FromConstructorSource
    {
        public int Value { get; set; }
    }

    private class FromConstructorTarget(FromConstructorSource fromConstructorSource)
    {
        public int Value { get; } = fromConstructorSource.Value;
    }

    private class TestNoSuitableConstructorClass(int value)
    {
        public int Value { get; } = value;
    }
}