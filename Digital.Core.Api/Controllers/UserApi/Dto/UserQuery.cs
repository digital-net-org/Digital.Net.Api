using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Mvc.Controllers.Pagination;

namespace Digital.Core.Api.Controllers.UserApi.Dto;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}