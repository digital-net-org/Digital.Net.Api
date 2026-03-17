using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class MediaPayload
{
    [Required]
    public required string Name { get; set; }

    public string? Alt { get; set; }
}
