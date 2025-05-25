using Digital.Net.Api.Controllers.Generic.Pagination;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PuckConfigQuery : Query
{
    public string? Version { get; set; }
}