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

    [Test]
    [Arguments("/blog/:slug", "/blog/jambon-beurre", true)]
    [Arguments("/blog/:slug", "/blog", false)]
    [Arguments("/blog/:slug", "/blog/a/b", false)]
    [Arguments("/blog/:slug", "/shop/jambon", false)]
    [Arguments("/blog/:slug", "/blog/", false)]
    [Arguments("/blog/:slug", "/blog/:slug", true)]
    [Arguments("/:section/:id", "/docs/42", true)]
    [Arguments("/:section/:id", "/docs", false)]
    [Arguments("/docs/:a/pages/:b", "/docs/x/pages/y", true)]
    [Arguments("/docs/:a/pages/:b", "/docs/x/wrong/y", false)]
    [Arguments("/static", "/static", true)]
    [Arguments("/static", "/other", false)]
    [Arguments("/", "/", true)]
    [Arguments("", "/blog/a", false)]
    [Arguments("/blog/:slug", "", false)]
    public async Task IsPatternMatch_CoversPath(string pattern, string path, bool expected)
    {
        var result = PagePathAnalyzer.IsPatternMatch(pattern, path);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("/", 0)]
    [Arguments("/blog", 0)]
    [Arguments("/blog/:slug", 1)]
    [Arguments("/:section/:id", 2)]
    [Arguments("", 0)]
    public async Task CountDynamicSlugs_CountsSegments(string path, int expected)
    {
        var result = PagePathAnalyzer.CountDynamicSlugs(path);
        await Assert.That(result).IsEqualTo(expected);
    }
}
