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
    }

    public string? PuckData { get; set; }
}