using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Cms.Services.Articles.Dto;
using Digital.Net.Core.Http.Services.Crud;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Services.Articles;

public class ArticleRelatedEnricher(CmsContext context) : IDtoEnricher<Article, ArticleDto>
{
    public async Task EnrichAsync(Article entity, ArticleDto dto, CancellationToken ct)
    {
        dto.Related = await context.ArticleRelated
            .AsNoTracking()
            .Where(p => p.ParentId == entity.Id)
            .OrderBy(p => p.Order)
            .Select(p => new ArticleRefDto { Id = p.Child.Id, Title = p.Child.Title })
            .ToListAsync(ct);
    }
}
