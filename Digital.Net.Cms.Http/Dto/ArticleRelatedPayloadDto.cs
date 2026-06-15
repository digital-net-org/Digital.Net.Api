using Digital.Net.Cms.Models.Articles;
using Digital.Net.Lib.Entities.Exceptions;
using Digital.Net.Lib.Entities.Pivots;

namespace Digital.Net.Cms.Http.Dto;

public class ArticleRelatedPayloadDto : IPivotPayload<ArticleRelatedPayloadDto, ArticleRelated, Article>
{
    public ArticleRelatedPayloadDto()
    {
    }

    public ArticleRelatedPayloadDto(ArticleRelated pivot)
    {
        Id = pivot.ChildId;
    }

    public Guid? Id { get; set; }

    public Article ToChild() =>
        throw new EntityValidationException(
            "/related: Articles cannot be created from here; use the \"cms/articles\" API."
        );

    public void ApplyTo(Article child)
    {
    }
}
