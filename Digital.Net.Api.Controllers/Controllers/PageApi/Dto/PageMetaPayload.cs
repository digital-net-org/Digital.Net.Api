using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageMetaPayload
{
    [Required]
    public string Key { get; set; }

    [Required]
    public string Value { get; set; }
 
    [Required]
    public Guid PageId { get; set; }
    
    [Required]
    public string Content { get; set; }
}