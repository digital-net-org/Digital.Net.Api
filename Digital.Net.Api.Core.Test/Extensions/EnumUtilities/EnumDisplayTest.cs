using System.ComponentModel.DataAnnotations;
using Digital.Net.Api.Core.Extensions.EnumUtilities;
using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Core.Test.Extensions.EnumUtilities;

public class EnumDisplayTest : UnitTest
{
    [Test]
    public async Task GetDisplayName_ReturnsEnumDisplayName_WhenAttributeIsSet() =>
        await Assert.That(ETest.TestEnumValue.GetDisplayName()).IsEqualTo("Test of very simple case");

    [Test]
    public async Task GetDisplayName_ReturnsEmptyString_WhenAttributeIsNotSet() =>
        await Assert.That(ETest.TestEnumValue2.GetDisplayName()).IsEqualTo(string.Empty);


    [Test]
    public async Task GetEnumDisplayNames_ReturnsEnumDisplayNames()
    {
        var test = EnumDisplay.GetEnumDisplayNames<ETest>().ToArray();
        await Assert.That(test.Length).IsEqualTo(2);
        await Assert.That(test[0]).IsEqualTo("Test of very simple case");
        await Assert.That(test[1]).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ToReferenceString_ReturnsCorrectString() =>
        await Assert.That(ETest.TestEnumValue.ToReferenceString()).IsEqualTo("TEST_ENUM_VALUE");

    private enum ETest
    {
        [Display(Name = "Test of very simple case")]
        TestEnumValue,
        TestEnumValue2
    }
}
