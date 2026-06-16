using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Lib.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Services;

public abstract class MediaGalleryEnricher<TParent, TParentDto, TPivot, TPivotDto>(
    CmsContext cmsContext,
    DigitalContext digitalContext
) : IDtoEnricher<TParent, TParentDto>
    where TParent : Entity
    where TParentDto : class
    where TPivot : Pivot<TParent, Media>
    where TPivotDto : MediaDto
{
    protected abstract Guid GetParentId(TParentDto dto);
    protected abstract void SetMedia(TParentDto dto, List<TPivotDto> media);
    protected abstract TPivotDto Project(TPivot pivot, MediaDto media);

    public async Task EnrichAsync(TParentDto dto, CancellationToken ct)
    {
        var pivots = await cmsContext.Set<TPivot>()
            .Include(p => p.Child)
            .AsNoTracking()
            .Where(p => p.ParentId == GetParentId(dto))
            .OrderBy(p => p.Order)
            .ToListAsync(ct);

        if (pivots.Count == 0)
        {
            SetMedia(dto, []);
            return;
        }

        var documentIds = pivots.Select(p => p.Child.DocumentId).Distinct().ToList();
        var documents = await digitalContext.Documents
            .AsNoTracking()
            .Where(d => documentIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, ct);

        SetMedia(dto, pivots
            .Where(p => documents.ContainsKey(p.Child.DocumentId))
            .Select(p => Project(p, new MediaDto(p.Child, documents[p.Child.DocumentId])))
            .ToList());
    }
}