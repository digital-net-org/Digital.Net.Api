using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Safari.Net.TestTools;
using SafariDigital.Core.Application;

namespace Tests.SafariDigital.Core.Application;

public class ApplicationSettingsTest : UnitTest
{
    private readonly IConfiguration _configuration = new ConfigurationBuilder().AddInMemoryCollection(
        new Dictionary<string, string> { { "Section", "Value" } }!
    ).Build();

    [Fact]
    public void GetSection_Success()
    {
        var result = _configuration.GetSection<string>(ETestAppSettings.TestSetting);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetSection_Failure()
    {
        var result = _configuration.GetSection<string>(ETestAppSettings.TestSetting2);
        Assert.Null(result);
    }

    private enum ETestAppSettings
    {
        [Display(Name = "Section")] TestSetting,
        TestSetting2
    }
}