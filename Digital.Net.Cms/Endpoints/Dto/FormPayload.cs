using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class FormPayload
{
    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? SubmitLabel { get; set; }
}
