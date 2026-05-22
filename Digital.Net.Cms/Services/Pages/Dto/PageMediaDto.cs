using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Services.Pages.Dto;

public class PageMediaDto : MediaDto
{
    public PageMediaDto()
    {
    }

    public PageMediaDto(PageMedia pivot, MediaDto media)
    {
        Id = pivot.ChildId;
        Label = pivot.Label;
        Name = media.Name;
        Alt = media.Alt;
        Published = media.Published;
        DocumentId = media.DocumentId;
        Width = media.Width;
        Height = media.Height;
        FileSize = media.FileSize;
        MimeType = media.MimeType;
        CreatedAt = media.CreatedAt;
        UpdatedAt = media.UpdatedAt;
    }

    public string? Label { get; set; }
}