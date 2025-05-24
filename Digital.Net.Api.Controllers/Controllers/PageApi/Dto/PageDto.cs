using Digital.Net.Api.Controllers.Controllers.ViewApi.Dto;
using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Models.Views;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageDto
{
    public PageDto()
    {
    }

    public PageDto(Page page)
    {
        Id = page.Id;
        Title = page.Title;
        Path = page.Path;
        IsPublished = page.IsPublished;
        ViewId = page.ViewId;
        View = page.View is not null ? Mapper.MapFromConstructor<View, ViewLightDto>(page.View) : null;
        CreatedAt = page.CreatedAt;
        UpdatedAt = page.UpdatedAt;
    }

    public Guid? Id { get; init; }
    public string? Title { get; set; }
    public string? Path { get; set; }
    public bool? IsPublished { get; set; }
    public Guid? ViewId { get; set; }
    public ViewLightDto? View { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}