using Digital.Net.Cms.Models.Articles;

namespace Digital.Net.Cms.Http.Dto;

public class ArticlePublicMediaDto
{
    public ArticlePublicMediaDto()
    {
    }

    public ArticlePublicMediaDto(ArticleMedia pivot, MediaDto media)
    {
        Id = pivot.ChildId;
        Label = pivot.Label;
        Alt = media.Alt;
    }

    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Alt { get; set; } = string.Empty;
}