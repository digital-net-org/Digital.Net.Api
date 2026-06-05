using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Cms.Http.Dto;

public class TagQuery : Query
{
    public string? Name { get; set; }
}