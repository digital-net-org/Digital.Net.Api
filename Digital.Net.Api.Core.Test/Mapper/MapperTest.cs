using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Core.Test.Mapper;

public class MapperTests : UnitTest
{
    [Fact]
    public void Map_ReturnsInstanceOfTargetType_WhenMatchingPropertiesFound()
    {
        var source = new Source { Value = 42 };
        var result = Models.Mapper.Map<Source, Target>(source);
        Assert.NotNull(result);
        Assert.IsAssignableFrom<Target>(result);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Map_ReturnsInstanceOfTargetType_WhenMatchingPropertiesNotFound()
    {
        var source = new Source { Value = 42 };
        var result = Models.Mapper.Map<Source, TargetFail>(source);
        Assert.NotNull(result);
        Assert.IsAssignableFrom<TargetFail>(result);
        Assert.Equal(string.Empty, result.NotANumber);
    }

    [Fact]
    public void Map_ReturnsListOfTargetType_WhenMatchingPropertiesFound()
    {
        var source = new List<Source>
        {
            new() { Value = 42 },
            new() { Value = 43 },
            new() { Value = 44 }
        };
        var result = Models.Mapper.Map<Source, Target>(source);
        Assert.NotNull(result);
        Assert.IsAssignableFrom<List<Target>>(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(42, result[0].Value);
        Assert.Equal(43, result[1].Value);
        Assert.Equal(44, result[2].Value);
    }

    [Fact]
    public void MapFromConstructor_ReturnsInstanceOfTargetType_WhenSuitableConstructorFound()
    {
        var source = new FromConstructorSource { Value = 42 };
        var result = Models.Mapper.MapFromConstructor<FromConstructorSource, FromConstructorTarget>(source);
        Assert.NotNull(result);
        Assert.IsAssignableFrom<FromConstructorTarget>(result);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void MapFromConstructor_ThrowsInvalidOperationException_WhenNoSuitableConstructorFound()
    {
        var source = new FromConstructorSource { Value = 42 };
        Assert.Throws<InvalidOperationException>(
            () => Models.Mapper.MapFromConstructor<FromConstructorSource, TestNoSuitableConstructorClass>(source));
    }

    [Fact]
    public void MapFromConstructor_ReturnsListOfTargetType_WhenSuitableConstructorFound()
    {
        var source = new List<FromConstructorSource>
        {
            new() { Value = 42 },
            new() { Value = 43 },
            new() { Value = 44 }
        };
        var result = Models.Mapper.MapFromConstructor<FromConstructorSource, FromConstructorTarget>(source);
        Assert.NotNull(result);
        Assert.IsAssignableFrom<List<FromConstructorTarget>>(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(42, result[0].Value);
        Assert.Equal(43, result[1].Value);
        Assert.Equal(44, result[2].Value);
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