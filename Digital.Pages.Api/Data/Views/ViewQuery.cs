using Digital.Lib.Net.Mvc.Controllers.Pagination;

namespace Digital.Pages.Api.Data.Views;

public class ViewQuery : Query
{
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
}