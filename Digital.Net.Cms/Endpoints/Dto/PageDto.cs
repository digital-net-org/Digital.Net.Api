using Digital.Net.Cms.Models;

namespace Digital.Net.Cms.Endpoints.Dto;

public class PageDto
{
    public PageDto()
    {
    }

    public PageDto(Page page)
    {
        Id = page.Id;
        Path = page.Path;
        Published = page.Published;
        Indexed = page.Indexed;
        Title = page.Title;
        Description = page.Description;
        JsonLd = page.JsonLd;
        OpenGraph = page.OpenGraph;
        Redirect = page.Redirect;
        CreatedAt = page.CreatedAt;
        UpdatedAt = page.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string Path { get; set; } = string.Empty;
    public bool Published { get; set; }
    public bool Indexed { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? JsonLd { get; set; }
    public string? OpenGraph { get; set; }
    public string? Redirect { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
