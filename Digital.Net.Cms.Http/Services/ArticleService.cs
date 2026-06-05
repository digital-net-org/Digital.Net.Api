using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Services;

public class ArticleService(
    CmsContext context,
    IEnumerable<IDtoEnricher<Article, ArticleDto>> enrichers
)
{
    public async Task<Result<bool>> GetSlugAvailability(string slug, Guid? excludeId, CancellationToken ct = default)
    {
        var result = new Result<bool>();
        try
        {
            var taken = await context.Articles
                .AnyAsync(a => a.Slug == slug && (excludeId == null || a.Id != excludeId), ct);
            result.Value = !taken;
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }


    public async Task<Result<ArticleDto>> GetArticleBySlug(string slug, CancellationToken ct = default)
    {
        var result = new Result<ArticleDto>();
        try
        {
            var article = await context.Articles
                .AsNoTracking()
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.Slug == slug && a.PublishedAt != null, ct);

            if (article is null)
                throw new ResourceNotFoundException();

            var dto = new ArticleDto(article);
            foreach (var enricher in enrichers)
                await enricher.EnrichAsync(article, dto, ct);
            result.Value = dto;
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }
}