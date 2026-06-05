using Digital.Net.Cms.Models.Articles;

namespace Digital.Net.Cms.Http.Dto;

public class ArticleListDto
{
    public ArticleListDto()
    {
    }

    public ArticleListDto(Article article)
    {
        Id = article.Id;
        Title = article.Title;
        Slug = article.Slug;
        PublishedAt = article.PublishedAt;
        PageId = article.PageId;
        CreatedAt = article.CreatedAt;
        UpdatedAt = article.UpdatedAt;
        Tags = article.Tags.Select(t => new TagDto(t)).ToList();
    }

    public Guid Id { get; init; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public Guid? PageId { get; set; }
    public List<TagDto> Tags { get; set; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
