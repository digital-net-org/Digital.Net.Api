using Digital.Net.Cms.Models;

namespace Digital.Net.Cms.Endpoints.Dto;

public class ArticleDto : PageDto
{
    public ArticleDto()
    {
    }

    public ArticleDto(Article article) : base(article)
    {
        Name = article.Name;
        Content = article.Content;
        Tags = article.Tags.Select(t => new TagDto(t)).ToList();
    }

    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<TagDto> Tags { get; set; } = [];
}
