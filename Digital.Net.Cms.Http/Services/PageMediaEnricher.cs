using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Http.Services.Crud;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Services;

public class PageMediaEnricher(CmsContext cmsContext, DigitalContext digitalContext)
    : IDtoEnricher<Page, PageDto>
{
    public async Task EnrichAsync(PageDto dto, CancellationToken ct)
    {
        var pivots = await cmsContext.PageMedia
            .Include(p => p.Child)
            .AsNoTracking()
            .Where(p => p.ParentId == dto.Id)
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