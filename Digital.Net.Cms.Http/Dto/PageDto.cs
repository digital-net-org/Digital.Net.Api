using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Http.Dto;

public class PageDto
{
    public PageDto()
    {
    }

    public Guid Id { get; init; }
    public string Path { get; set; } = string.Empty;
    public PageEntityType? EntityType { get; set; }
    public bool Published { get; set; }
    public bool Indexed { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? JsonLd { get; set; }
    public string? Redirect { get; set; }
    public List<PageMediaDto> Media { get; set; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
