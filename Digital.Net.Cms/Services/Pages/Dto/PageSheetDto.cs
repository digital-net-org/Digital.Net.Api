using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Services.Pages.Dto;

public class PageSheetDto
{
    public PageSheetDto()
    {
    }

    public PageSheetDto(PageSheet pivot)
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
}
