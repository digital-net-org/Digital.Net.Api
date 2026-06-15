namespace Digital.Net.Cms.Http.Dto;

public class FormListDto
{
    public FormListDto()
    {
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Published { get; set; }
    public string SubmitLabel { get; set; } = "Submit";
    public string? Path { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}