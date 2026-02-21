using Digital.Net.Api.Core.Extensions.EnumUtilities;
using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Core.Test.Extensions.EnumUtilities;

public class EnumValuesTest : UnitTest
{
    private enum ETest
    {
        Test,
        Test2
    }

    [Test]
    public async Task GetEnumValues_ReturnsEnumValues()
    {
        var test = EnumDisplay.GetEnumValues<ETest>().ToArray();
        await Assert.That(test.Length).IsEqualTo(2);
        await Assert.That(test[0]).IsEqualTo(ETest.Test);
        await Assert.That(test[1]).IsEqualTo(ETest.Test2);
    }
}