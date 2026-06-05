using Digital.Net.Cms.Models.Articles;

namespace Digital.Net.Cms.Http.Dto;

public class ArticleMediaDto : MediaDto
{
    public ArticleMediaDto()
    {
    }

    public ArticleMediaDto(ArticleMedia pivot, MediaDto media)
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

    public string Label { get; set; } = string.Empty;
}