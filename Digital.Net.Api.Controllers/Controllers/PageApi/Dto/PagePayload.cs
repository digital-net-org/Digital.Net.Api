using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PagePayload
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Path { get; set; }
    
    public int PuckConfigId { get; set; }
}