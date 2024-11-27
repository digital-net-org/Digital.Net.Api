using Digital.Net.Core.Messages;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Services.Authentication;
using SafariDigital.Services.Authentication.Models;

namespace SafariDigital.Api.Controllers;

[ApiController, Route("[controller]")]
public class AuthenticationController(IConfiguration configuration, IAuthenticationService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<Result<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await authService.Login(request.Login, request.Password);
        return result.HasError || result.Value is null ? Unauthorized(result) : Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<Result<LoginResponse>>> RefreshTokens()
    {
        var result = await authService.RefreshTokens();
        return result.HasError ? Unauthorized(result) : Ok(result);
    }

    [HttpPost("logout"), Authorize(Role = EUserRole.User)]
    public async Task<IActionResult> Logout()
    {
        await authService.Logout();
        return NoContent();
    }

    [HttpPost("logout-all"), Authorize(Role = EUserRole.User)]
    public async Task<IActionResult> LogoutAll()
    {
        await authService.LogoutAll();
        return NoContent();
    }

    [HttpPost("password/generate"), ApiKey]
    public ActionResult<string> GeneratePassword([FromBody] string password) =>
        configuration.GetPasswordRegex().IsMatch(password)
            ? Ok(authService.GeneratePassword(password))
            : BadRequest("Password does not meet the requirements.");

    [HttpGet("role/visitor/test")]
    public IActionResult TestVisitorAuthorization() => NoContent();

    [HttpGet("role/user/test"), Authorize(Role = EUserRole.User)]
    public IActionResult TestUserAuthorization() => NoContent();

    [HttpGet("role/admin/test"), Authorize(Role = EUserRole.Admin)]
    public IActionResult TestAdminAuthorization() => NoContent();

    [HttpGet("role/super-admin/test"), Authorize(Role = EUserRole.SuperAdmin)]
    public IActionResult TestSuperAdminAuthorization() => NoContent();
}