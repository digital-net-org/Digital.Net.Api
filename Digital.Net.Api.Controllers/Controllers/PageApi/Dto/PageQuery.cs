using Digital.Net.Api.Controllers.Generic.Pagination;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageQuery : Query
{
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
}