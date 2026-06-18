using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Cms.Http.Dto;

public class ArticlePublicQuery : Query
{
    public string? Name { get; set; }
    public Guid? PageId { get; set; }
}