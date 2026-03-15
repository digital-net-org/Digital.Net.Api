using Digital.Net.Cms.Models;

namespace Digital.Net.Cms.Endpoints.Dto;

public class TagDto
{
    public TagDto()
    {
    }

    public TagDto(Tag tag)
    {
        Id = tag.Id;
        Name = tag.Name;
        Color = tag.Color;
        CreatedAt = tag.CreatedAt;
        UpdatedAt = tag.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
