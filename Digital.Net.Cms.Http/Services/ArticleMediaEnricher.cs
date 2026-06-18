using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Core.Entities.Context;

namespace Digital.Net.Cms.Http.Services;

public class ArticleMediaEnricher(
    CmsContext cmsContext,
    DigitalContext digitalContext
)
    : MediaGalleryEnricher<Article, ArticleDto, ArticleMedia, ArticleMediaDto>(cmsContext, digitalContext)
{
    protected override Guid GetParentId(ArticleDto dto) => dto.Id;
    protected override void SetMedia(ArticleDto dto, List<ArticleMediaDto> media) => dto.Media = media;
    protected override ArticleMediaDto Project(ArticleMedia pivot, MediaDto media) => new(pivot, media);
}
