using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Core.Entities.Pivots;

namespace Digital.Net.Cms.Http.Dto;

public class PageSheetPayloadDto : IPivotPayload<PageSheetPayloadDto, PageSheet, Sheet>
{
    public PageSheetPayloadDto()
    {
    }

    public PageSheetPayloadDto(PageSheet pivot)
    {
        Id = pivot.ChildId;
        Name = pivot.Child.Name;
        Type = pivot.Child.Type;
        Content = pivot.Child.Content;
        Published = pivot.Child.Published;
    }

    public Guid? Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Content { get; set; }
    public bool Published { get; set; }

    public Sheet ToChild() => new()
    {
        Name = Name,
        Type = Type,
        Content = Content,
        Published = Published
    };

    public void ApplyTo(Sheet child)
    {
        child.Name = Name;
        child.Type = Type;
        child.Content = Content;
        child.Published = Published;
    }
}
