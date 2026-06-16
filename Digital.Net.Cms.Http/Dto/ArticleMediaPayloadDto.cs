using Digital.Net.Cms.Models.Articles;

namespace Digital.Net.Cms.Http.Dto;

public class ArticleMediaPayloadDto : MediaPivotPayloadDto<ArticleMediaPayloadDto, ArticleMedia>
{
    public override void ApplyToPivot(ArticleMedia pivot) => pivot.Label = Label.Trim();
}
