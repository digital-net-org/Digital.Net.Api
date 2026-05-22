using Digital.Net.Cms.Context;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Services.Medias;

public class MediaLabelService(
    CmsContext context
)
{
    public async Task<Result<List<string>>> GetExistingLabels(string? search, CancellationToken ct)
    {
        var result = new Result<List<string>>();
        try
        {
            var articleLabels = context.ArticleMedia
                .Where(p => p.Label != null && p.Label != "")
                .Select(p => p.Label!);
            var pageLabels = context.PageMedia
                .Where(p => p.Label != null && p.Label != "")
                .Select(p => p.Label!);

            var query = articleLabels.Union(pageLabels);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var needle = search.Trim().ToLowerInvariant();
                query = query.Where(l => l.Contains(needle));
            }

            result.Value = await query.Distinct().OrderBy(l => l).ToListAsync(ct);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }
}