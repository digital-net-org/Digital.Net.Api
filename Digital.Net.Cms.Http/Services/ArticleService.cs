using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Core.Http.Services.Pagination;
using Digital.Net.Core.Http.Services.Pagination.Extensions;
using Digital.Net.Lib.Entities.Projection;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Predicates;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Services;

public class ArticleService(
    CmsContext context
)
{
    private static readonly Expression<Func<ArticleMedia, ArticlePublicMediaDto>> ToMediaDto =
        x => new ArticlePublicMediaDto { Id = x.ChildId, Label = x.Label, Alt = x.Child.Alt };
    
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


    public async Task<Result<ArticlePublicDto>> GetArticleBySlug(string slug, CancellationToken ct = default)
    {
        var result = new Result<ArticlePublicDto>();
        try
        {
            var dto = await context.Articles
                .Where(a => a.Slug == slug && a.PublishedAt != null)
                .ProjectTo<Article, ArticlePublicDto>()
                .FirstOrDefaultAsync(ct);

            if (dto is null)
                throw new ResourceNotFoundException();

            dto.Medias = await context.ArticleMedia
                .AsNoTracking()
                .Where(p => p.ParentId == dto.Id)
                .OrderBy(p => p.Order)
                .Select(ToMediaDto)
                .ToListAsync(ct);

            dto.Related = await context.ArticleRelated
                .AsNoTracking()
                .Where(p => p.ParentId == dto.Id)
                .OrderBy(p => p.Order)
                .Select(p => new ArticlePublicListDto
                {
                    Id = p.Child.Id,
                    Title = p.Child.Title,
                    Slug = p.Child.Slug,
                    PublishedAt = p.Child.PublishedAt,
                    UpdatedAt = p.Child.UpdatedAt,
                    Tags = p.Child.Tags.Select(x => new TagPublicDto { Name = x.Name, Color = x.Color }).ToList(),
                    Medias = context.ArticleMedia
                        .Where(m => m.ParentId == p.ChildId)
                        .OrderBy(m => m.Order)
                        .Select(ToMediaDto)
                        .ToList()
                })
                .ToListAsync(ct);

            result.Value = dto;
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<QueryResult<ArticlePublicListDto>> GetPublishedArticles(
        ArticlePublicQuery query,
        CancellationToken ct = default
    )
    {
        query.ValidateParameters();
        var result = new QueryResult<ArticlePublicListDto>();
        try
        {
            var predicate = PredicateBuilder.New<Article>()
                .ApplyDateRange(query)
                .Add(a => a.PublishedAt != null);
            if (!string.IsNullOrEmpty(query.Name))
                predicate = predicate.Add(a =>
                    EF.Functions.ILike(a.Title, $"{query.Name}%") ||
                    a.Tags.Any(b => EF.Functions.ILike(b.Name, $"{query.Name}%"))
                );
            if (query.PageId.HasValue)
                predicate = predicate.Add(a => a.PageId == query.PageId);

            var items = context.Articles.Where(predicate);
            result.Total = await items.CountAsync(ct);

            var config = new ParsingConfig { IsCaseSensitive = false };
            var orderClause = OrderByResolver.ResolveOrderClause<Article>(query.OrderBy, query.Order);

            result.Value = await items
                .AsNoTracking()
                .OrderBy(config, orderClause)
                .Skip((query.Index - 1) * query.Size)
                .Take(query.Size)
                .Select(a => new ArticlePublicListDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Slug = a.Slug,
                    PublishedAt = a.PublishedAt,
                    UpdatedAt = a.UpdatedAt,
                    Tags = a.Tags.Select(t => new TagPublicDto { Name = t.Name, Color = t.Color }).ToList(),
                    Medias = context.ArticleMedia
                        .Where(m => m.ParentId == a.Id)
                        .OrderBy(m => m.Order)
                        .Select(ToMediaDto)
                        .ToList()
                })
                .ToListAsync(ct);

            result.Index = query.Index;
            result.Size = query.Size;
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }
}