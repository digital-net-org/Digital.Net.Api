namespace Digital.Net.Cms.Http.Dto;

public class TagDto
{
    public TagDto()
    {
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
