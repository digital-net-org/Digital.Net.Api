using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Authentication.Controllers;
using Digital.Lib.Net.Authentication.Services.Authentication;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Data.Models.Users;

namespace SafariDigital.Api.Controllers;

[ApiController, Route("authentication/user")]
public class AuthenticationController(
    IAuthenticationService<User> authenticationService
) : AuthenticationController<User>(authenticationService)
{
    [HttpGet("role/0/test"), Authorize(AuthorizeType.Jwt)]
    public IActionResult TestUserAuthorization(UserRole role) => NoContent();

    [HttpGet("role/1/test"), Authorize(AuthorizeType.Jwt, UserRole.Admin)]
    public IActionResult TestAdminAuthorization() => NoContent();

    [HttpGet("role/2/test"), Authorize(AuthorizeType.Jwt, UserRole.SuperAdmin)]
    public IActionResult TestSuperAdminAuthorization() => NoContent();
}