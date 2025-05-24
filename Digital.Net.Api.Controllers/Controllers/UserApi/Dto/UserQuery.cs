using Digital.Net.Api.Controllers.Generic.Pagination;

namespace Digital.Net.Api.Controllers.Controllers.UserApi.Dto;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}