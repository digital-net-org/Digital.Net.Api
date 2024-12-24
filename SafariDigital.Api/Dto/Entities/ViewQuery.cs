using Digital.Net.Mvc.Controllers.Pagination;

namespace SafariDigital.Api.Dto.Entities;

public class ViewQuery : Query
{
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
}