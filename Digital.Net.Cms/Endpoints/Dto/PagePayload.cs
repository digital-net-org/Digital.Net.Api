using System.ComponentModel.DataAnnotations;
using Digital.Net.Cms.Models;

namespace Digital.Net.Cms.Endpoints.Dto;

public class PagePayload
{
    [Required]
    public required string Path { get; set; }

    public PageEntityType? EntityType { get; set; }
}
