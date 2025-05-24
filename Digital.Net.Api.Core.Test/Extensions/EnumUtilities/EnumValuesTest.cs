using Digital.Net.Api.Core.Extensions.EnumUtilities;
using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Core.Test.Extensions.EnumUtilities;

public class EnumValuesTest : UnitTest
{
    private enum ETest
    {
        Test,
        Test2
    }

    [Fact]
    public void GetEnumValues_ReturnsEnumValues() =>
        Assert.Equal([ETest.Test, ETest.Test2], EnumDisplay.GetEnumValues<ETest>());
}