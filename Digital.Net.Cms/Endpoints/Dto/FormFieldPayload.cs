using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class FormFieldPayload
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Type { get; set; }

    [Required]
    public required string Label { get; set; }

    public string? Placeholder { get; set; }

    public string? DefaultValue { get; set; }

    public bool Required { get; set; }

    public int SortOrder { get; set; }

    public string? ValidationJson { get; set; }

    public string? OptionsJson { get; set; }
}
