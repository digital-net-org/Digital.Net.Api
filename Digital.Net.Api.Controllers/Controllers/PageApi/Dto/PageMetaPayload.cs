using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageMetaPayload
{
    public string? Name { get; set; }
    
    public string? Property { get; set; }
 
    [Required]
    public Guid PageId { get; set; }
    
    [Required]
    public string Content { get; set; }
}