using Microsoft.AspNetCore.Mvc;
using Safari.Net.Core.Messages;
using SafariDigital.Api.Attributes;
using SafariDigital.Data.Models.Database;
using SafariDigital.Services.Authentication;
using SafariDigital.Services.Authentication.Models;

namespace SafariDigital.Api.Controllers;

[ApiController]
public class AuthController(IConfiguration configuration, IAuthenticationService authService) : ControllerBase
{
    [HttpPost("/authentication/login")]
    public async Task<ActionResult<Result<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await authService.Login(request.Login, request.Password);
        return result.HasError || result.Value is null ? Unauthorized(result) : Ok(result);
    }

    [HttpPost("/authentication/refresh")]
    public async Task<ActionResult<Result<LoginResponse>>> RefreshTokens()
    {
        var result = await authService.RefreshTokens();
        return result.HasError ? Unauthorized(result) : Ok(result);
    }

    [HttpPost("/authentication/logout"), Authorize(Role = EUserRole.User)]
    public IActionResult Logout()
    {
        authService.Logout();
        return Ok();
    }

    [HttpPost("/authentication/logout-all"), Authorize(Role = EUserRole.User)]
    public IActionResult LogoutAll()
    {
        authService.LogoutAll();
        return Ok();
    }

    [HttpPost("/authentication/password/generate"), ApiKey]
    public IActionResult GeneratePassword([FromBody] string password) =>
        configuration.GetPasswordRegex().IsMatch(password)
            ? Ok(authService.GeneratePassword(password))
            : BadRequest("Password does not meet the requirements.");

    [HttpGet("/authentication/role/visitor/test")]
    public IActionResult TestVisitorAuthorization() => Ok();

    [HttpGet("/authentication/role/user/test"), Authorize(Role = EUserRole.User)]
    public IActionResult TestUserAuthorization() => Ok();

    [HttpGet("/authentication/role/admin/test"), Authorize(Role = EUserRole.Admin)]
    public IActionResult TestAdminAuthorization() => Ok();

    [HttpGet("/authentication/role/super-admin/test"), Authorize(Role = EUserRole.SuperAdmin)]
    public IActionResult TestSuperAdminAuthorization() => Ok();
}