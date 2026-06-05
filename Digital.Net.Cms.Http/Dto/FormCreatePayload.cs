using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Http.Dto;

public class FormCreatePayload
{
    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string SubmitLabel { get; set; } = "Submit";

    public string? Path { get; set; }
}