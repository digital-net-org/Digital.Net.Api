using Digital.Net.Cms.Services.Pages;
using Digital.Net.Tests.Core;

namespace Digital.Net.Tests.Cms.Services.Pages;

public class PagePathAnalyzerTest : UnitTest
{
    [Test]
    [Arguments("/", false)]
    [Arguments("/home", false)]
    [Arguments("/home/nested", false)]
    [Arguments("/:id", true)]
    [Arguments("/articles/:slug", true)]
    [Arguments("/articles/:slug/comments/:commentId", true)]
    [Arguments("/:_underscored", true)]
    [Arguments("/:123bad", false)]
    [Arguments("", false)]
    public async Task HasDynamicSlug_Detects_Correctly(string path, bool expected)
    {
        var result = PagePathAnalyzer.HasDynamicSlug(path);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task HasDynamicSlug_ReturnsFalse_ForNull()
    {
        var result = PagePathAnalyzer.HasDynamicSlug(null);
        await Assert.That(result).IsFalse();
    }
}
