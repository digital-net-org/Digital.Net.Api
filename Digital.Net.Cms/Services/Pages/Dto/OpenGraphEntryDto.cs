using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Services.Pages.Dto;

public class OpenGraphEntryDto
{
    public OpenGraphEntryDto()
    {
    }

    public OpenGraphEntryDto(PageOpenGraph pivot)
    {
        Id = pivot.ChildId;
        Property = pivot.Child.Property;
        Content = pivot.Child.Content;
    }

    public Guid? Id { get; set; }
    public required string Property { get; set; }
    public required string Content { get; set; }
}