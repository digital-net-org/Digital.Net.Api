using Digital.Net.Lib.Entities.Attributes;

namespace Digital.Net.Cms.Http.Dto;

public class FormPublicDto
{
    public FormPublicDto()
    {
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SubmitLabel { get; set; } = "Submit";
    [ProjectOrderBy(nameof(FormFieldDto.SortOrder))]
    public List<FormFieldDto> Fields { get; set; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
