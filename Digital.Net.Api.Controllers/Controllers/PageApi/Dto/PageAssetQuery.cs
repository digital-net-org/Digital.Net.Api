using Digital.Net.Api.Controllers.Generic.Pagination;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageAssetQuery : Query
{
    public string Path { get; set; }
    public string MimeType { get; set; }
}