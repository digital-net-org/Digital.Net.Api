using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class FormSubmitPayload
{
    public Dictionary<string, string?> Values { get; set; } = [];

    [Required]
    public required string SubmitterIp { get; set; }

    [Required]
    public required string UserAgent { get; set; }
}
