using Digital.Net.Cms.Models.Articles;

namespace Digital.Net.Cms.Services.Articles.Dto;

public class ArticleRefDto
{
    public ArticleRefDto()
    {
    }

    public ArticleRefDto(Article article)
    {
        Id = article.Id;
        Title = article.Title;
    }

    public Guid Id { get; init; }

    public string Title { get; set; } = string.Empty;
}
