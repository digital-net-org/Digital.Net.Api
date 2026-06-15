namespace Digital.Net.Cms.Http.Dto;

public class ArticleListDto
{
    public ArticleListDto()
    {
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
