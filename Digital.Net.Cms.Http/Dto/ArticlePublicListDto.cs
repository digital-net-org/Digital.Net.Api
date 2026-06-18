namespace Digital.Net.Cms.Http.Dto;

public class ArticlePublicListDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; init; }
    public List<TagPublicDto> Tags { get; set; } = [];
    public List<ArticlePublicMediaDto> Medias { get; set; } = [];
}