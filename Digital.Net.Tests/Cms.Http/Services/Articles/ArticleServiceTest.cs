using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Http.Services;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Tests.Core;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Cms.Http.Services.Articles;

public class ArticleServiceTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private CmsContext _context = null!;
    private ArticleService _service = null!;

    public async Task InitializeAsync()
    {
        await DbFixture.EnsureCreatedAsync<CmsContext>();
        _context = DbFixture.CreateContext<CmsContext>();
        _service = new ArticleService(_context);
    }

    private void AttachMedia(Article article, string label, string? alt, int order)
    {
        var media = new Media { Name = label, Alt = alt, DocumentId = Guid.NewGuid(), Published = true };
        _context.Media.Add(media);
        _context.SaveChanges();
        _context.ArticleMedia.Add(new ArticleMedia
        {
            ParentId = article.Id,
            ChildId = media.Id,
            Label = label,
            Order = order
        });
        _context.SaveChanges();
    }

    [Test]
    public async Task GetPublishedArticles_ShouldExcludeDrafts()
    {
        var tag = _context.BuildTestTag("scope-drafts-" + Guid.NewGuid().ToString("N")[..6]);
        _context.BuildTestArticle(published: true, tags: [tag]);
        _context.BuildTestArticle(published: false, tags: [tag]);

        var result = await _service.GetPublishedArticles(new ArticlePublicQuery { Name = tag.Name });

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Total).IsEqualTo(1);
        await Assert.That(result.Value.Single().PublishedAt).IsNotNull();
    }

    [Test]
    public async Task GetPublishedArticles_ShouldProjectTagsAndMedias()
    {
        var tag = _context.BuildTestTag("scope-medias-" + Guid.NewGuid().ToString("N")[..6]);
        var article = _context.BuildTestArticle(published: true, tags: [tag]);
        AttachMedia(article, "cover", "cover alt", 0);
        AttachMedia(article, "thumbnail", null, 1);

        var result = await _service.GetPublishedArticles(new ArticlePublicQuery { Name = tag.Name });

        await Assert.That(result.HasError).IsFalse();
        var dto = result.Value.Single();
        await Assert.That(dto.Tags.Any(t => t.Name == tag.Name)).IsTrue();
        await Assert.That(dto.Medias.Count).IsEqualTo(2);
        var cover = dto.Medias.First(m => m.Label == "cover");
        await Assert.That(cover.Alt).IsEqualTo("cover alt");
        await Assert.That(cover.Id).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task GetPublishedArticles_ShouldPaginate()
    {
        var tag = _context.BuildTestTag("scope-page-" + Guid.NewGuid().ToString("N")[..6]);
        for (var i = 0; i < 3; i++)
            _context.BuildTestArticle(published: true, tags: [tag]);

        var page1 = await _service.GetPublishedArticles(new ArticlePublicQuery
            { Name = tag.Name, Index = 1, Size = 2 });
        var page2 = await _service.GetPublishedArticles(new ArticlePublicQuery
            { Name = tag.Name, Index = 2, Size = 2 });

        await Assert.That(page1.Total).IsEqualTo(3);
        await Assert.That(page1.Count).IsEqualTo(2);
        await Assert.That(page1.End).IsFalse();
        await Assert.That(page2.Count).IsEqualTo(1);
        await Assert.That(page2.End).IsTrue();
    }

    [Test]
    public async Task GetArticleBySlug_ShouldProjectOwnMediasAndRelatedMedias()
    {
        var article = _context.BuildTestArticle(published: true);
        AttachMedia(article, "cover", "main cover", 0);

        var related = _context.BuildTestArticle(published: true);
        AttachMedia(related, "related-cover", "related alt", 0);
        _context.ArticleRelated.Add(new ArticleRelated { ParentId = article.Id, ChildId = related.Id, Order = 0 });
        _context.SaveChanges();

        var result = await _service.GetArticleBySlug(article.Slug);

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value!.Medias.Single().Label).IsEqualTo("cover");
        var relatedDto = result.Value.Related.Single();
        await Assert.That(relatedDto.Id).IsEqualTo(related.Id);
        await Assert.That(relatedDto.Medias.Single().Alt).IsEqualTo("related alt");
    }
}