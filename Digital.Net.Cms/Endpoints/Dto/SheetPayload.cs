using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class SheetPayload
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Type { get; set; }

    [Required]
    public required string Content { get; set; }
}
