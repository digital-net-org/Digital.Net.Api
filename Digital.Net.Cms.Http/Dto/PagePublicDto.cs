using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Http.Dto;

public class PagePublicDto
{
    public PagePublicDto()
    {
    }

    public PagePublicDto(Page page)
    {
        Id = page.Id;
        Indexed = page.Indexed;
        Title = page.Title;
        Description = page.Description;
        JsonLd = page.JsonLd;
        Redirect = page.Redirect;
    }

    public Guid Id { get; init; }
    public bool Indexed { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? JsonLd { get; set; }
    public string? Redirect { get; set; }
    public List<OpenGraphEntryPublicDto> OpenGraph { get; set; } = [];
}