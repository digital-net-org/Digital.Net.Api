using Digital.Net.Cms.Http.Services;
using Digital.Net.Tests.Core;

namespace Digital.Net.Tests.Cms.Http.Services.Pages;

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

    [Test]
    [Arguments("/articles/:slug", "my-article", "/articles/my-article")]
    [Arguments("/:id", "42", "/42")]
    [Arguments("/static", "x", "/static")]
    [Arguments("/blog/:cat/:slug", "shared", "/blog/shared/shared")]
    public async Task ResolveDynamicPath_SubstitutesParams(string pattern, string value, string expected)
    {
        var result = PagePathAnalyzer.ResolveDynamicPath(pattern, value);
        await Assert.That(result).IsEqualTo(expected);
    }
}
