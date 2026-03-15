using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class PagePayload
{
    [Required]
    public required string Path { get; set; }
}
