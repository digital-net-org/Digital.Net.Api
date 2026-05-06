using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Cms.Services.Pages;
using Digital.Net.Cms.Services.Pages.Dto;
using Digital.Net.Cms.Services.Pages.Exceptions;
using Digital.Net.Tests.Core;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Cms.Services.Pages;

public class PagePublicServiceTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private CmsContext _context = null!;
    private PagePublicService _service = null!;

    public async Task InitializeAsync()
    {
        await DbFixture.EnsureCreatedAsync<CmsContext>();
        _context = DbFixture.CreateContext<CmsContext>();
        _service = new PagePublicService(_context);
    }

    private static PageBuildPayload Build(string path, PageEntityType? type = null, string? slug = null) =>
        new() { Path = path, PageType = type, PageSlug = slug };

    private (string pattern, string slug) SeedArticleAndTemplatedPage(
        string articleTitle = "Hello World",
        string articleDescription = "Greetings from prod",
        string? pageTitle = "Blog: {{ article.title }}",
        string? pageDescription = "Read: {{ article.description }}"
    )
    {
        var slug = "tpl-" + Guid.NewGuid().ToString("N")[..8];
        var pattern = $"/blog-{TestId}-{Guid.NewGuid().ToString("N")[..6]}/:slug";

        _context.Articles.Add(new Article
        {
            Slug = slug,
            Title = articleTitle,
            Description = articleDescription,
            Content = "body",
            PublishedAt = DateTime.UtcNow
        });
        _context.Pages.Add(new Page
        {
            Path = pattern,
            Published = true,
            Indexed = true,
            EntityType = PageEntityType.Article,
            Title = pageTitle,
            Description = pageDescription
        });
        _context.SaveChanges();
        return (pattern, slug);
    }

    [Test]
    public async Task BuildPage_ShouldReturnPublishedPage()
    {
        var path = "/static-" + Guid.NewGuid().ToString("N")[..8];
        _context.BuildTestPage(path, true);

        var result = await _service.BuildPublicPage(Build(path));

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value).IsNotNull();
    }

    [Test]
    public async Task BuildPage_ShouldReturnInvalidPagePath_WhenPageIsNotPublished()
    {
        var path = "/unpub-" + Guid.NewGuid().ToString("N")[..8];
        _context.BuildTestPage(path);

        var result = await _service.BuildPublicPage(Build(path));

        await Assert.That(result.HasErrorOfType<InvalidPagePathException>()).IsTrue();
    }

    [Test]
    public async Task BuildPage_ShouldReturnInvalidPagePath_WhenPageDoesNotExist()
    {
        var result = await _service.BuildPublicPage(Build("/missing-" + Guid.NewGuid().ToString("N")[..8]));

        await Assert.That(result.HasErrorOfType<InvalidPagePathException>()).IsTrue();
    }

    [Test]
    public async Task BuildPage_ShouldReturnInvalidPagePath_WhenPathIsEmpty()
    {
        var result = await _service.BuildPublicPage(Build(string.Empty));

        await Assert.That(result.HasErrorOfType<InvalidPagePathException>()).IsTrue();
    }

    [Test]
    public async Task BuildPage_HydratesTemplateVariables_FromMatchingArticle()
    {
        var (pattern, slug) = SeedArticleAndTemplatedPage();

        var result = await _service.BuildPublicPage(Build(pattern, PageEntityType.Article, slug));

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value!.Title).IsEqualTo("Blog: Hello World");
        await Assert.That(result.Value!.Description).IsEqualTo("Read: Greetings from prod");
    }

    [Test]
    public async Task BuildPage_LeavesPlaceholdersUnchanged_WhenSourceFieldUnknown()
    {
        var (pattern, slug) = SeedArticleAndTemplatedPage(
            pageTitle: "T",
            pageDescription: "Junk: {{ article.unknown }}"
        );

        var result = await _service.BuildPublicPage(Build(pattern, PageEntityType.Article, slug));

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value!.Description).IsEqualTo("Junk: {{ article.unknown }}");
    }

    [Test]
    public async Task BuildPage_ReturnsInvalidPagePath_WhenTemplatedPage_ButArticleNotFound()
    {
        var pattern = $"/no-article-{TestId}/:slug";
        _context.Pages.Add(new Page
        {
            Path = pattern,
            Published = true,
            Indexed = true,
            EntityType = PageEntityType.Article,
            Title = "Blog: {{ article.title }}"
        });
        _context.SaveChanges();

        var result = await _service.BuildPublicPage(
            Build(pattern, PageEntityType.Article, "no-such-article-" + Guid.NewGuid().ToString("N")[..8]));

        await Assert.That(result.HasErrorOfType<InvalidPagePathException>()).IsTrue();
    }

    [Test]
    public async Task BuildPage_DoesNotHydrate_WhenPayloadMissesSlug()
    {
        // Page templatée + payload sans PageSlug : on retourne la page telle quelle, sans
        // hydrater. Le client a déclaré le bon PageType, mais ne demande pas d'instance —
        // utile pour récupérer le squelette de la page.
        var pattern = $"/no-slug-{TestId}/:slug";
        _context.Pages.Add(new Page
        {
            Path = pattern,
            Published = true,
            Indexed = true,
            EntityType = PageEntityType.Article,
            Title = "Blog: {{ article.title }}"
        });
        _context.SaveChanges();

        var result = await _service.BuildPublicPage(Build(pattern, PageEntityType.Article));

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value!.Title).IsEqualTo("Blog: {{ article.title }}");
    }

    [Test]
    public async Task BuildPage_ReturnsInvalidPageType_WhenPayloadEntityTypeMismatchesPage()
    {
        var slug = "mis-" + Guid.NewGuid().ToString("N")[..8];
        var pattern = $"/mismatch-{TestId}/:slug";
        _context.Articles.Add(new Article
        {
            Slug = slug,
            Title = "T",
            Description = "D",
            Content = "C",
            PublishedAt = DateTime.UtcNow
        });
        _context.Pages.Add(new Page
        {
            Path = pattern,
            Published = true,
            Indexed = true,
            EntityType = PageEntityType.Article,
            Title = "Blog: {{ article.title }}"
        });
        _context.SaveChanges();

        var result = await _service.BuildPublicPage(Build(pattern, PageEntityType.Form, slug));

        await Assert.That(result.HasErrorOfType<InvalidPageTypeException>()).IsTrue();
    }

    [Test]
    public async Task BuildPage_ReturnsInvalidPageType_WhenStaticPage_ButPayloadProvidesPageType()
    {
        var path = "/static-pt-" + Guid.NewGuid().ToString("N")[..8];
        _context.BuildTestPage(path, true);

        var result = await _service.BuildPublicPage(Build(path, PageEntityType.Article, "some-slug"));

        await Assert.That(result.HasErrorOfType<InvalidPageTypeException>()).IsTrue();
    }

    [Test]
    public async Task BuildPage_NotTemplated_DoesNotAlterValues()
    {
        var path = "/raw-" + Guid.NewGuid().ToString("N")[..8];
        _context.Pages.Add(new Page
        {
            Path = path,
            Published = true,
            Indexed = true,
            Title = "Plain {{ article.title }} text",
            Description = "No hydration"
        });
        _context.SaveChanges();

        var result = await _service.BuildPublicPage(Build(path));

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value!.Title).IsEqualTo("Plain {{ article.title }} text");
        await Assert.That(result.Value!.Description).IsEqualTo("No hydration");
    }
}
