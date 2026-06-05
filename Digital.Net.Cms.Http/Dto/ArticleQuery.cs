using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Cms.Http.Dto;

public class ArticleQuery : Query
{
    public string? Name { get; set; }
    public bool? Published { get; set; }
    public Guid? TagId { get; set; }
    public Guid? PageId { get; set; }
}