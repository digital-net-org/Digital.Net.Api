using Digital.Net.Core.Services.Pagination;

namespace Digital.Net.Cms.Endpoints.Dto;

public class ArticleQuery : Query
{
    public string? Name { get; set; }
    public bool? Published { get; set; }
    public Guid? TagId { get; set; }
}