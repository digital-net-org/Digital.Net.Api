using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Cms.Services.Articles.Dto;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Services.Articles;

/// <summary>
///     Article-specific query operations that don't fit the generic CRUD pipeline — slug
///     uniqueness probe and slug-based public lookup. Both are scoped here so the endpoints
///     stay thin and the slug logic lives in one place.
/// </summary>
public class ArticleService(
    CmsContext context,
    IEnumerable<IDtoEnricher<Article, ArticleDto>> enrichers
)
{
    /// <summary>
    ///     Returns <c>true</c> when <paramref name="slug"/> is not yet attached to any article.
    ///     <paramref name="excludeId"/> scopes out a specific article (edition case — the current
    ///     article's own slug is still considered available).
    /// </summary>
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

    /// <summary>
    ///     Resolves a published article by its slug. Hydrates the DTO through every registered
    ///     <see cref="IDtoEnricher{Article,ArticleDto}"/> so the response shape matches the
    ///     standard <c>GET cms/articles/{id}</c>. Returns a result carrying
    ///     <see cref="ResourceNotFoundException"/> when no published article matches.
    /// </summary>
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
