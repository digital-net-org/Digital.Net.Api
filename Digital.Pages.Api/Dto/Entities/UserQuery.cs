using Digital.Lib.Net.Mvc.Controllers.Pagination;
using Digital.Pages.Data.Models.Users;

namespace Digital.Pages.Api.Dto.Entities;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}