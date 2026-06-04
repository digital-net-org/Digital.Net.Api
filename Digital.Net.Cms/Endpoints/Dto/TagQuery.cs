using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Cms.Endpoints.Dto;

public class TagQuery : Query
{
    public string? Name { get; set; }
}