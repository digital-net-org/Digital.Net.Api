using System.ComponentModel.DataAnnotations;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Lib.Entities.Exceptions;
using Digital.Net.Lib.Entities.Pivots;

namespace Digital.Net.Cms.Http.Dto;

public class PageMediaPayloadDto : IPivotPayload<PageMediaPayloadDto, PageMedia, Media>
{
    public PageMediaPayloadDto()
    {
    }

    public PageMediaPayloadDto(PageMedia pivot)
    {
        Id = pivot.ChildId;
        Label = pivot.Label;
    }

    public Guid? Id { get; set; }

    [Required]
    public required string Label { get; set; }

    public Media ToChild() =>
        throw new EntityValidationException(
            "/media: Media creation require a file upload; upload it first using the \"cms/media\" API."
        );

    public void ApplyTo(Media child) =>
        throw new EntityValidationException(
            "/media: Media cannot be mutated from here; use the \"cms/media\" API."
        );

    public void ApplyToPivot(PageMedia pivot) => pivot.Label = Label.Trim();
}