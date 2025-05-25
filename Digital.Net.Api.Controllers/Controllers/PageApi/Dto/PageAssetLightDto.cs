using Digital.Net.Api.Entities.Models.Pages;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageAssetLightDto
{
    public PageAssetLightDto()
    {
    }

    public PageAssetLightDto(PageAsset pageAsset)
    {
        Id = pageAsset.Id;
        Path = pageAsset.Path;
        MimeType = pageAsset.Document?.MimeType ?? "application/octet-stream";
        CreatedAt = pageAsset.CreatedAt;
        UpdatedAt = pageAsset.UpdatedAt;
    }

    public int Id { get; init; }
    public string Path { get; set; }
    public string MimeType { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}