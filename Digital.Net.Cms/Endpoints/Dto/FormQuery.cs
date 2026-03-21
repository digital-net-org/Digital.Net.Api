using Digital.Net.Core.Services.Pagination;

namespace Digital.Net.Cms.Endpoints.Dto;

public class FormQuery : Query
{
    public string? Name { get; set; }
    public bool? Published { get; set; }
}
