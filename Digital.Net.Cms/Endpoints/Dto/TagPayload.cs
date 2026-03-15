using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class TagPayload
{
    [Required]
    public required string Name { get; set; }

    public string? Color { get; set; }
}
