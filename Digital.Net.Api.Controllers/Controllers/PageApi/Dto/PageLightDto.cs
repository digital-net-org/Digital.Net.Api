using Digital.Net.Api.Entities.Models.Pages;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageLightDto
{
    public PageLightDto()
    {
    }

    public PageLightDto(Page page)
    {
        Id = page.Id;
        Title = page.Title;
        Path = page.Path;
        IsPublished = page.IsPublished;
        Version = page.PuckConfig.Version;
        CreatedAt = page.CreatedAt;
        UpdatedAt = page.UpdatedAt;
    }

    public Guid? Id { get; init; }
    public string? Title { get; set; }
    public string? Path { get; set; }
    public string Version { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}