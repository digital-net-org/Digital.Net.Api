using Digital.Net.Core.Services.Pagination;

namespace Digital.Net.Cms.Endpoints.Dto;

public class SheetQuery : Query
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public bool? Published { get; set; }
}
