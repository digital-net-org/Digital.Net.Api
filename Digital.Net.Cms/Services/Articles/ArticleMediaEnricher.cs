using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Services.Crud;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Services.Articles;

public class ArticleMediaEnricher(
    CmsContext cmsContext,
    DigitalContext digitalContext
) : IDtoEnricher<Article, ArticleDto>
{
    public async Task EnrichAsync(Article entity, ArticleDto dto, CancellationToken ct)
    {
        var pivots = await cmsContext.ArticleMedia
            .Include(p => p.Child)
            .AsNoTracking()
            .Where(p => p.ParentId == entity.Id)
            .OrderBy(p => p.Order)
            .ToListAsync(ct);

        if (pivots.Count == 0)
        {
            dto.Media = [];
            return;
        }

        var documentIds = pivots.Select(p => p.Child.DocumentId).Distinct().ToList();
        var documents = await digitalContext.Documents
            .AsNoTracking()
            .Where(d => documentIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, ct);

        dto.Media = pivots
            .Where(p => documents.ContainsKey(p.Child.DocumentId))
            .Select(p => new ArticleMediaDto(p, new MediaDto(p.Child, documents[p.Child.DocumentId])))
            .ToList();
    }
}