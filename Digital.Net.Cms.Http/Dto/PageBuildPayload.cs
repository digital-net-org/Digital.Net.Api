using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Http.Dto;

public class PageBuildPayload
{
    public required string Path { get; set; }
    public PageEntityType? PageType { get; set; }
    public string? PageSlug { get; set; }
}