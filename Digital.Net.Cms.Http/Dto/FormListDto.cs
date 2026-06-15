namespace Digital.Net.Cms.Http.Dto;

public class FormListDto
{
    public FormListDto()
    {
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public bool Published { get; set; }
    public string? Path { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}