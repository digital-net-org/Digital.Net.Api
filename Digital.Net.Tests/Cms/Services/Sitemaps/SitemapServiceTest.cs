using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Cms.Services.Sitemaps;
using Digital.Net.Tests.Core;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Microsoft.EntityFrameworkCore;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Cms.Services.Sitemaps;

public class SitemapServiceTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private CmsContext _context = null!;
    private SitemapService _service = null!;

    public async Task InitializeAsync()
    {
        await DbFixture.EnsureCreatedAsync<CmsContext>();
        _context = DbFixture.CreateContext<CmsContext>();
        _service = new SitemapService(_context);
    }

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..(prefix.Length + 9)];

    [Test]
    public async Task GetEntries_ShouldIncludeStaticPublishedAndIndexedPage()
    {
        var path = "/" + Unique("static");
        _context.BuildTestPage(path: path, published: true, indexed: true);

        var entries = await _service.GetEntriesAsync();

        await Assert.That(entries.Any(e => e.Path == path)).IsTrue();
    }

    [Test]
    public async Task GetEntries_ShouldExcludeUnpublishedPage()
    {
        var path = "/" + Unique("unpub");
        _context.BuildTestPage(path: path, published: false, indexed: true);

        var entries = await _service.GetEntriesAsync();

        await Assert.That(entries.Any(e => e.Path == path)).IsFalse();
    }

    [Test]
    public async Task GetEntries_ShouldExcludeNonIndexedPage()
    {
        var path = "/" + Unique("noindex");
        _context.BuildTestPage(path: path, published: true, indexed: false);

        var entries = await _service.GetEntriesAsync();

        await Assert.That(entries.Any(e => e.Path == path)).IsFalse();
    }

    [Test]
    public async Task GetEntries_ShouldExpandDynamicArticlePage()
    {
        var prefix = "/" + Unique("art-pub");
        var pattern = $"{prefix}/:slug";
        _context.BuildTestPage(path: pattern, published: true, indexed: true, entityType: PageEntityType.Article);

        var slugA = Unique("a");
        var slugB = Unique("b");
        _context.BuildTestArticle(slug: slugA, published: true);
        _context.BuildTestArticle(slug: slugB, published: true);

        var entries = await _service.GetEntriesAsync();

        await Assert.That(entries.Any(e => e.Path == $"{prefix}/{slugA}")).IsTrue();
        await Assert.That(entries.Any(e => e.Path == $"{prefix}/{slugB}")).IsTrue();
        await Assert.That(entries.Any(e => e.Path == pattern)).IsFalse();
    }

    [Test]
    public async Task GetEntries_ShouldExcludeUnpublishedArticlesFromDynamicExpansion()
    {
        var prefix = "/" + Unique("art-unp");
        var pattern = $"{prefix}/:slug";
        _context.BuildTestPage(path: pattern, published: true, indexed: true, entityType: PageEntityType.Article);

        var draftSlug = Unique("draft");
        _context.BuildTestArticle(slug: draftSlug, published: false);

        var entries = await _service.GetEntriesAsync();

        await Assert.That(entries.Any(e => e.Path == $"{prefix}/{draftSlug}")).IsFalse();
    }

    [Test]
    public async Task GetEntries_ShouldNotExpandDynamicPage_WhenPageIsNotPublished()
    {
        var prefix = "/" + Unique("dyn-unp");
        var pattern = $"{prefix}/:slug";
        _context.BuildTestPage(path: pattern, published: false, indexed: true, entityType: PageEntityType.Article);

        var slug = Unique("hidden");
        _context.BuildTestArticle(slug: slug, published: true);

        var entries = await _service.GetEntriesAsync();

        await Assert.That(entries.Any(e => e.Path == $"{prefix}/{slug}")).IsFalse();
    }

    [Test]
    public async Task GetEntries_ShouldNotExpandDynamicPage_WhenPageIsNotIndexed()
    {
        var prefix = "/" + Unique("dyn-nox");
        var pattern = $"{prefix}/:slug";
        _context.BuildTestPage(path: pattern, published: true, indexed: false, entityType: PageEntityType.Article);

        var slug = Unique("masked");
        _context.BuildTestArticle(slug: slug, published: true);

        var entries = await _service.GetEntriesAsync();

        await Assert.That(entries.Any(e => e.Path == $"{prefix}/{slug}")).IsFalse();
    }

    [Test]
    public async Task GetEntries_ShouldUseMaxUpdatedAtBetweenPageAndArticle()
    {
        var prefix = "/" + Unique("upd");
        var pattern = $"{prefix}/:slug";
        var page = _context.BuildTestPage(path: pattern, published: true, indexed: true, entityType: PageEntityType.Article);

        var slug = Unique("entry");
        var article = _context.BuildTestArticle(slug: slug, published: true);

        var pageDate = DateTime.UtcNow.AddDays(2);
        var articleDate = DateTime.UtcNow.AddDays(1);
        await _context.Pages.Where(p => p.Id == page.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.UpdatedAt, pageDate));
        await _context.Articles.Where(a => a.Id == article.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.UpdatedAt, articleDate));

        var entries = await _service.GetEntriesAsync();
        var entry = entries.FirstOrDefault(e => e.Path == $"{prefix}/{slug}");

        await Assert.That(entry).IsNotNull();
        await Assert.That(entry!.UpdatedAt > articleDate).IsTrue();
    }

    [Test]
    public async Task GetEntries_ShouldIgnoreDynamicPage_WhenEntityTypeIsNotHandled()
    {
        var prefix = "/" + Unique("form");
        var pattern = $"{prefix}/:slug";
        _context.BuildTestPage(path: pattern, published: true, indexed: true, entityType: PageEntityType.Form);

        var entries = await _service.GetEntriesAsync();

        await Assert.That(entries.Any(e => e.Path.StartsWith(prefix))).IsFalse();
    }
}
