using Digital.Net.Mvc.Controllers.Pagination;
using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Api.Controllers.UserApi.Dto;

public class UserQuery : Query
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public EUserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}