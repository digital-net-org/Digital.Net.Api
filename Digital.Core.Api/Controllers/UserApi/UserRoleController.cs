using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Core.Api.Controllers.UserApi;

[ApiController, Route("user/role")]
public class UserRoleController(
) : ControllerBase
{
    [HttpGet("0/test"), Authorize(AuthorizeType.Any)]
    public IActionResult TestUserAuthorization(UserRole role) => NoContent();

    [HttpGet("1/test"), Authorize(AuthorizeType.Any, UserRole.Admin)]
    public IActionResult TestAdminAuthorization() => NoContent();

    [HttpGet("2/test"), Authorize(AuthorizeType.Any, UserRole.SuperAdmin)]
    public IActionResult TestSuperAdminAuthorization() => NoContent();
}