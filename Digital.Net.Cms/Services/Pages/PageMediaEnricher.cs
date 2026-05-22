using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Cms.Services.Pages.Dto;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Services.Crud;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Services.Pages;

public class PageMediaEnricher(CmsContext cmsContext, DigitalContext digitalContext)
    : IDtoEnricher<Page, PageDto>
{
    public async Task EnrichAsync(Page entity, PageDto dto, CancellationToken ct)
    {
        var pivots = await cmsContext.PageMedia
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
            .Select(p => new PageMediaDto(p, new MediaDto(p.Child, documents[p.Child.DocumentId])))
            .ToList();
    }
}