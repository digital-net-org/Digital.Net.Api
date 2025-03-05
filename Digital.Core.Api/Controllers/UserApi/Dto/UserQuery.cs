using Digital.Lib.Net.Mvc.Controllers.Pagination;

namespace Digital.Core.Api.Controllers.UserApi.Dto;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}