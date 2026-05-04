using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Cms.Services.Pages;
using Digital.Net.Lib.Date;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Services.Sitemaps;

public class SitemapService(CmsContext context)
{
    public async Task<List<SitemapEntryDto>> GetEntriesAsync()
    {
        var pages = await context.Pages
            .AsNoTracking()
            .Where(p => p.Published && p.Indexed)
            .ToListAsync();

        var entries = new List<SitemapEntryDto>();
        foreach (var page in pages)
        {
            if (!PagePathAnalyzer.HasDynamicSlug(page.Path))
            {
                entries.Add(new SitemapEntryDto { Path = page.Path, UpdatedAt = page.UpdatedAt });
                continue;
            }
            entries.AddRange(await ResolveDynamicPageAsync(page));
        }

        return entries;
    }

    private async Task<List<SitemapEntryDto>> ResolveDynamicPageAsync(Page page) =>
        page.EntityType switch
        {
            PageEntityType.Article => await ResolveArticleEntriesAsync(page),
            _ => []
        };

    private async Task<List<SitemapEntryDto>> ResolveArticleEntriesAsync(Page page)
    {
        var articles = await context.Articles
            .AsNoTracking()
            .Where(a => a.PublishedAt != null)
            .Select(a => new { a.Slug, a.UpdatedAt })
            .ToListAsync();

        return articles
            .Select(a => new SitemapEntryDto
            {
                Path = PagePathAnalyzer.ResolveDynamicPath(page.Path, a.Slug),
                UpdatedAt = DateTimeResolver.MaxUpdatedAt(a.UpdatedAt, page.UpdatedAt)
            })
            .ToList();
    }
}
