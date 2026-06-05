using Digital.Net.Cms.Models.Forms;

namespace Digital.Net.Cms.Http.Dto;

public class FormFieldDto
{
    public FormFieldDto()
    {
    }

    public FormFieldDto(FormField field)
    {
        Id = field.Id;
        FormId = field.FormId;
        Name = field.Name;
        Type = field.Type;
        Label = field.Label;
        Placeholder = field.Placeholder;
        DefaultValue = field.DefaultValue;
        Required = field.Required;
        SortOrder = field.SortOrder;
        ValidationJson = field.ValidationJson;
        OptionsJson = field.OptionsJson;
        CreatedAt = field.CreatedAt;
        UpdatedAt = field.UpdatedAt;
    }

    public Guid Id { get; init; }
    public Guid FormId { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public string? DefaultValue { get; set; }
    public bool Required { get; set; }
    public int SortOrder { get; set; }
    public string? ValidationJson { get; set; }
    public string? OptionsJson { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
