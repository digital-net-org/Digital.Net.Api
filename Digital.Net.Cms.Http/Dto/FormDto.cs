using Digital.Net.Core.Entities.Attributes;

namespace Digital.Net.Cms.Http.Dto;

public class FormDto
{
    public FormDto()
    {
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Published { get; set; }
    public string SubmitLabel { get; set; } = "Submit";
    public string? Path { get; set; }
    [ProjectOrderBy(nameof(FormFieldDto.SortOrder))]
    public List<FormFieldDto> Fields { get; set; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
