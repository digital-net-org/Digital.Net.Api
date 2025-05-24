using Digital.Net.Api.Core.Extensions.StringUtilities;
using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Core.Test.Extensions.StringUtilities;

public class StringConverterTest : UnitTest
{
    [Fact]
    public void ToSnakeCase_ReturnsFormattedString_WhenPascalCase()
    {
        var result = "PascalCase".ToSnakeCase();
        Assert.Equal("pascal_case", result);
    }

    [Fact]
    public void ToSnakeCase_ReturnsFormattedString_WhenCamelCase()
    {
        var result = "camelCase".ToSnakeCase();
        Assert.Equal("camel_case", result);
    }

    [Fact]
    public void ToUpperSnakeCase_ReturnsFormattedString_WhenPascalCase()
    {
        var result = "PascalCase".ToUpperSnakeCase();
        Assert.Equal("PASCAL_CASE", result);
    }
}