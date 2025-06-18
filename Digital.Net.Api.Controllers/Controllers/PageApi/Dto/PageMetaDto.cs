using Digital.Net.Api.Entities.Models.Pages;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageMetaDto
{
    public PageMetaDto()
    {
    }

    public PageMetaDto(PageMeta pageMeta)
    {
        Id = pageMeta.Id;
        Content = pageMeta.Content;
        Name = pageMeta.Name;
        Property = pageMeta.Property;
        CreatedAt = pageMeta.CreatedAt;
        UpdatedAt = pageMeta.UpdatedAt;
    }

    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Property { get; init; }
    public string Content { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}