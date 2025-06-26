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
        PageId = pageMeta.PageId;
        Content = pageMeta.Content;
        Key = pageMeta.Key;
        Value = pageMeta.Value;
        CreatedAt = pageMeta.CreatedAt;
        UpdatedAt = pageMeta.UpdatedAt;
    }

    public Guid Id { get; init; }
    public Guid PageId { get; init; }
    public string Key { get; init; }
    public string Value { get; init; }
    public string Content { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}