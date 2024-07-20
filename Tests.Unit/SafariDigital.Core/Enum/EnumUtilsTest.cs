using System.ComponentModel.DataAnnotations;
using SafariDigital.Core.Enum;
using Tests.Core.Base;

namespace Tests.Unit.SafariDigital.Core.Enum;

public class EnumUtilsTest : UnitTest
{
    [Fact]
    public void GetDisplayName_ShouldReturnEnumDisplayName() =>
        Assert.Equal("Test of very simple case", ETest.Test.GetDisplayName());

    [Fact]
    public void GetDisplayName_ShouldReturnEmptyString() =>
        Assert.Equal(string.Empty, ETest.Test2.GetDisplayName());

    [Fact]
    public void GetEnumValues_ShouldReturnEnumValues() =>
        Assert.Equal(new[] { ETest.Test, ETest.Test2 }, EnumUtils.GetEnumValues<ETest>());

    [Fact]
    public void GetEnumDisplayNames_ShouldReturnEnumDisplayNames() =>
        Assert.Equal(new[] { "Test of very simple case", string.Empty }, EnumUtils.GetEnumDisplayNames<ETest>());

    private enum ETest
    {
        [Display(Name = "Test of very simple case")]
        Test,
        Test2
    }
}