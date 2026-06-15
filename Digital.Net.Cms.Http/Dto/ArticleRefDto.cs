namespace Digital.Net.Cms.Http.Dto;

public class ArticleRefDto
{
    public ArticleRefDto()
    {
    }

    public Guid Id { get; init; }

    public string Title { get; set; } = string.Empty;
}
