using Digital.Net.Api.Entities.Models.Pages;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageDto : PageLightDto
{
    public PageDto()
    {
    }

    public PageDto(Page page) : base(page)
    {
        PuckData = page.PuckData;
        Description = page.Description;
        IsIndexed = page.IsIndexed;
    }

    public string? Description { get; set; }
    public bool IsIndexed { get; set; }
    public string? PuckData { get; set; }
}