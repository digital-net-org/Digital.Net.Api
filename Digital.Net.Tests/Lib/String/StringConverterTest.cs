using Digital.Net.Lib.String;
using Digital.Net.Tests.Core;

namespace Digital.Net.Tests.Lib.String;

public class StringConverterTest : UnitTest
{
    [Test]
    public async Task ToSnakeCase_ReturnsFormattedString_WhenPascalCase()
    {
        var result = "PascalCase".ToSnakeCase();
        await Assert.That(result).IsEqualTo("pascal_case");
    }

    [Test]
    public async Task ToSnakeCase_ReturnsFormattedString_WhenCamelCase()
    {
        var result = "camelCase".ToSnakeCase();
        await Assert.That(result).IsEqualTo("camel_case");
    }

    [Test]
    public async Task ToUpperSnakeCase_ReturnsFormattedString_WhenPascalCase()
    {
        var result = "PascalCase".ToUpperSnakeCase();
        await Assert.That(result).IsEqualTo("PASCAL_CASE");
    }
}