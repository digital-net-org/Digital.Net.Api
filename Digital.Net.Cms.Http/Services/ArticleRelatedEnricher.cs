using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Core.Http.Services.Crud;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Services;

public class ArticleRelatedEnricher(CmsContext context) : IDtoEnricher<Article, ArticleDto>
{
    public async Task EnrichAsync(ArticleDto dto, CancellationToken ct) =>
        dto.Related = await context.ArticleRelated
            .AsNoTracking()
            .Where(p => p.ParentId == dto.Id)
            .OrderBy(p => p.Order)
            .Select(p => new ArticleRefDto { Id = p.Child.Id, Title = p.Child.Title })
            .ToListAsync(ct);
}