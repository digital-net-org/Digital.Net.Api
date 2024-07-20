using System.ComponentModel.DataAnnotations;
using SafariDigital.Core.Enum;

namespace Tests.Unit.SafariDigital.Core.Enum;

public class EnumExtensionTest
{
    [Fact]
    public void GetDisplayName_ShouldReturnEnumDisplayName() =>
        Assert.Equal("Test of very simple case", ETest.Test.GetDisplayName());

    [Fact]
    public void GetDisplayName_ShouldReturnEmptyString() =>
        Assert.Equal(string.Empty, ETest.Test2.GetDisplayName());

    private enum ETest
    {
        [Display(Name = "Test of very simple case")]
        Test,
        Test2
    }
}