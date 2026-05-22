using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Services.Medias.Dto;

public class MediaPayload
{
    [Required]
    public required string Name { get; set; }

    public string? Alt { get; set; }
}
