using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models.Pages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Services;

public class PageTemplateResolver(CmsContext context)
{
    /// <summary>
    ///     Resolves the published dynamic page whose pattern covers the given path. The most specific
    ///     pattern wins: fewest dynamic slugs, then longest path. Dynamic paths never inherit (no chains).
    /// </summary>
    public async Task<Page?> ResolveAsync(string? path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path) || PagePathAnalyzer.HasDynamicSlug(path))
            return null;

        var candidates = await context.Pages
            .AsNoTracking()
            .Where(p => p.Published && p.Path.Contains(":"))
            .ToListAsync(ct);

        return candidates
            .Where(p => PagePathAnalyzer.IsPatternMatch(p.Path, path))
            .OrderBy(p => PagePathAnalyzer.CountDynamicSlugs(p.Path))
            .ThenByDescending(p => p.Path.Length)
            .ThenBy(p => p.Path, StringComparer.Ordinal)
            .FirstOrDefault();
    }
}
