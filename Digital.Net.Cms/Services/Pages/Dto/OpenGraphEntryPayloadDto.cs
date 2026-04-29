using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Core.Entities.Pivots;

namespace Digital.Net.Cms.Services.Pages.Dto;

public class OpenGraphEntryPayloadDto : IPivotPayload<OpenGraphEntryPayloadDto, PageOpenGraph, OpenGraphEntry>
{
    public OpenGraphEntryPayloadDto()
    {
    }

    public OpenGraphEntryPayloadDto(PageOpenGraph pivot)
    {
        Id = pivot.ChildId;
        Property = pivot.Child.Property;
        Content = pivot.Child.Content;
    }

    public Guid? Id { get; set; }
    public required string Property { get; set; }
    public required string Content { get; set; }

    public OpenGraphEntry ToChild() => new()
    {
        Property = Property,
        Content = Content
    };

    public void ApplyTo(OpenGraphEntry child)
    {
        child.Property = Property;
        child.Content = Content;
    }
}
