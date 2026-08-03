using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Lib.Date;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Services;

public class SitemapService(CmsContext context)
{
    public async Task<List<SitemapEntryDto>> GetEntriesAsync()
    {
        var pages = await context.Pages
            .AsNoTracking()
            .Where(p => p.Published)
            .ToListAsync();

        // A published dedicated page owns its path: it decides its own sitemap presence,
        // so template expansions never re-list it (even when it opted out via Indexed).
        var dedicatedPaths = pages
            .Where(p => !PagePathAnalyzer.HasDynamicSlug(p.Path))
            .Select(p => p.Path)
            .ToHashSet();

        var entries = new List<SitemapEntryDto>();
        var seen = new HashSet<string>();
        foreach (var page in pages.Where(p => p.Indexed))
        {
            if (!PagePathAnalyzer.HasDynamicSlug(page.Path))
            {
                if (seen.Add(page.Path))
                    entries.Add(new SitemapEntryDto { Path = page.Path, UpdatedAt = page.UpdatedAt });
                continue;
            }
            entries.AddRange((await ResolveDynamicPageAsync(page))
                .Where(e => !dedicatedPaths.Contains(e.Path) && seen.Add(e.Path)));
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
            .Where(a => a.PublishedAt != null && a.PageId == page.Id)
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
