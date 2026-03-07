using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Controllers.Dto;

public class PagePayload
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Path { get; set; }
}