using System.ComponentModel.DataAnnotations;
using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Http.Dto;

public class PagePayload
{
    [Required]
    public required string Path { get; set; }

    public PageEntityType? EntityType { get; set; }
}
