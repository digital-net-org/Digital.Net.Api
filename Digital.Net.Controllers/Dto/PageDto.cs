using Digital.Net.Entities.Models.Pages;

namespace Digital.Net.Controllers.Dto;

public class PageDto : PageLightDto
{
    public PageDto()
    {
    }

    public PageDto(Page page) : base(page)
    {
        PuckData = page.JsonLd;
        Description = page.Description;
        IsIndexed = page.IsIndexed;
    }

    public string? Description { get; set; }
    public bool IsIndexed { get; set; }
    public string? PuckData { get; set; }
}