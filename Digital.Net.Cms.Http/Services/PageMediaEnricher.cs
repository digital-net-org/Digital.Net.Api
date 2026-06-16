using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Core.Entities.Context;

namespace Digital.Net.Cms.Http.Services;

public class PageMediaEnricher(CmsContext cmsContext, DigitalContext digitalContext)
    : MediaGalleryEnricher<Page, PageDto, PageMedia, PageMediaDto>(cmsContext, digitalContext)
{
    protected override Guid GetParentId(PageDto dto) => dto.Id;
    protected override void SetMedia(PageDto dto, List<PageMediaDto> media) => dto.Media = media;
    protected override PageMediaDto Project(PageMedia pivot, MediaDto media) => new(pivot, media);
}
