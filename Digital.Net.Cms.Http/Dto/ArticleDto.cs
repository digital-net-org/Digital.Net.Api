namespace Digital.Net.Cms.Http.Dto;

public class ArticleDto
{
    public ArticleDto()
    {
    }

    public Guid Id { get; init; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public Guid? PageId { get; set; }
    public List<TagDto> Tags { get; set; } = [];
    public List<ArticleMediaDto> Media { get; set; } = [];
    public List<ArticleRefDto> Related { get; set; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
