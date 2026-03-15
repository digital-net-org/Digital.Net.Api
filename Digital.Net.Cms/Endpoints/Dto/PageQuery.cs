using Digital.Net.Core.Services.Pagination;

namespace Digital.Net.Cms.Endpoints.Dto;

public class PageQuery : Query
{
    public string? Path { get; set; }
    public bool? Published { get; set; }
    public bool? Indexed { get; set; }
}