using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Http.Dto;

public class PageListDto
{
    public PageListDto()
    {
    }

    public Guid Id { get; init; }
    public string Path { get; set; } = string.Empty;
    public bool Published { get; set; }
    public bool Indexed { get; set; }
    public string? Title { get; set; }
    public string? Redirect { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
