using Digital.Net.Cms.Models;

namespace Digital.Net.Cms.Endpoints.Dto;

public class FormDto
{
    public FormDto()
    {
    }

    public FormDto(Form form)
    {
        Id = form.Id;
        Name = form.Name;
        Description = form.Description;
        Published = form.Published;
        SubmitLabel = form.SubmitLabel;
        Fields = form.Fields.OrderBy(f => f.SortOrder).Select(f => new FormFieldDto(f)).ToList();
        CreatedAt = form.CreatedAt;
        UpdatedAt = form.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Published { get; set; }
    public string SubmitLabel { get; set; } = "Submit";
    public List<FormFieldDto> Fields { get; set; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
