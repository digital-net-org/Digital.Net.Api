using Digital.Net.Api.Entities.Models.Pages;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PagePublicDto
{
    public PagePublicDto()
    {
    }

    public PagePublicDto(Page page)
    {
        Data = page.View?.Data;
        Version = page.View?.PuckConfig.Version;
    }

    public string? Data { get; set; }
    public string? Version { get; set; }
}