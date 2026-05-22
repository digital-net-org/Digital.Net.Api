using Digital.Net.Cms.Models.Articles;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Pivots;

namespace Digital.Net.Cms.Endpoints.Dto;

public class ArticleMediaPayloadDto : IPivotPayload<ArticleMediaPayloadDto, ArticleMedia, Media>
{
    public ArticleMediaPayloadDto()
    {
    }

    public ArticleMediaPayloadDto(ArticleMedia pivot)
    {
        Id = pivot.ChildId;
        Label = pivot.Label;
    }

    public Guid? Id { get; set; }
    public string? Label { get; set; }

    public Media ToChild() =>
        throw new EntityValidationException(
            "/media: Media creation require a file upload; upload it first using the \"cms/media\" API."
        );

    public void ApplyTo(Media child) =>
        throw new EntityValidationException(
            "/media: Media cannot be mutated from here; use the \"cms/media\" API."
        );

    public void ApplyToPivot(ArticleMedia pivot) =>
        pivot.Label = string.IsNullOrWhiteSpace(Label) ? null : Label.Trim();
}