using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Cms.Http.Dto;

public class FormQuery : Query
{
    public string? Name { get; set; }
    public bool? Published { get; set; }
    public string? Path { get; set; }
}