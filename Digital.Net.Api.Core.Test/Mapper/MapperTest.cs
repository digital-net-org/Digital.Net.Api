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
        await Assert.That(() =>
                Models.Mapper.MapFromConstructor<FromConstructorSource, TestNoSuitableConstructorClass>(source))
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

    [Test]
    public async Task TryMap_UsesConstructorMapping_WhenAvailable()
    {
        var source = new Source { Value = 10, Name = "Test" };
        var result = Models.Mapper.TryMap<Source, TargetWithConstructor>(source);
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Value).IsEqualTo(10);
        await Assert.That(result.Name).IsEqualTo("Test");
    }

    [Test]
    public async Task TryMap_UsesPropertyMapping_WhenConstructorMappingFails()
    {
        var source = new Source { Value = 20, Name = "Test 2" };
        var result = Models.Mapper.TryMap<Source, TargetWithProperties>(source);
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Value).IsEqualTo(20);
        await Assert.That(result.Name).IsEqualTo("Test 2");
    }

    [Test]
    public async Task TryMap_UsesConstructorMapping_ForList_WhenAvailable()
    {
        var source = new List<Source>
        {
            new() { Value = 1, Name = "A" },
            new() { Value = 2, Name = "B" }
        };
        var result = Models.Mapper.TryMap<Source, TargetWithConstructor>(source).ToList();
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result[0].Value).IsEqualTo(1);
        await Assert.That(result[0].Name).IsEqualTo("A");
        await Assert.That(result[1].Value).IsEqualTo(2);
        await Assert.That(result[1].Name).IsEqualTo("B");
    }

    [Test]
    public async Task TryMap_UsesPropertyMapping_ForList_WhenConstructorMappingFails()
    {
        var source = new List<Source>
        {
            new() { Value = 3, Name = "C" },
            new() { Value = 4, Name = "D" }
        };
        var result = Models.Mapper.TryMap<Source, TargetWithProperties>(source).ToList();
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result[0].Value).IsEqualTo(3);
        await Assert.That(result[0].Name).IsEqualTo("C");
        await Assert.That(result[1].Value).IsEqualTo(4);
        await Assert.That(result[1].Name).IsEqualTo("D");
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
        public int Value { get; set; } = fromConstructorSource.Value;
    }

    private class TargetWithConstructor
    {
        public int Value { get; set; }
        public string Name { get; set; }

        public TargetWithConstructor(Source source)
        {
            Value = source.Value;
            Name = source.Name;
        }
    }

    private class TargetWithProperties
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestNoSuitableConstructorClass(int value)
    {
        public int Value { get; set; } = value;  
    }
}