using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Core;
using SafariDigital.Data.Models.Database;
using SafariDigital.Services.Authentication;
using SafariDigital.Services.Authentication.Models;

namespace SafariDigital.Api.Controllers;

[ApiController]
public class AuthController(IConfiguration configuration, IAuthenticationService authService) : ControllerBase
{
    [HttpPost("/authentication/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.Login(request.Login, request.Password);
        return result.HasError || result.Value is null ? Unauthorized(result) : Ok(result.Value);
    }

    [HttpGet("/authentication/refresh")]
    public async Task<IActionResult> RefreshTokens()
    {
        var result = await authService.RefreshTokens();
        return result.HasError ? Unauthorized(result) : Ok(result.Value);
    }

    [Authorize(Role = EUserRole.User)]
    [HttpPost("/authentication/logout")]
    public IActionResult Logout()
    {
        authService.Logout();
        return Ok();
    }

    [Authorize(Role = EUserRole.User)]
    [HttpPost("/authentication/logout-all")]
    public IActionResult LogoutAll()
    {
        authService.LogoutAll();
        return Ok();
    }

    [HttpGet("/authentication/username/pattern")]
    public IActionResult GetUsernamePattern() => Ok(RegularExpressions.GetUsernameRegex().ToString());

    [HttpGet("/authentication/email/pattern")]
    public IActionResult GetEmailPattern() => Ok(RegularExpressions.GetEmailRegex().ToString());

    [Authorize(Role = EUserRole.User)]
    [HttpGet("/authentication/password/pattern")]
    public IActionResult GetPasswordPattern() => Ok(configuration.GetPasswordRegex().ToString());

    [Authorize(Role = EUserRole.SuperAdmin)]
    [HttpGet("/authentication/password/generate/{password:length(12, 64)}")]
    public IActionResult GeneratePassword(string password) => Ok(authService.GeneratePassword(password));

    [HttpGet("/authentication/role/visitor/test")]
    public IActionResult TestVisitorAuthorization() => Ok();

    [Authorize(Role = EUserRole.User)]
    [HttpGet("/authentication/role/user/test")]
    public IActionResult TestUserAuthorization() => Ok();

    [Authorize(Role = EUserRole.Admin)]
    [HttpGet("/authentication/role/admin/test")]
    public IActionResult TestAdminAuthorization() => Ok();

    [Authorize(Role = EUserRole.SuperAdmin)]
    [HttpGet("/authentication/role/super-admin/test")]
    public IActionResult TestSuperAdminAuthorization() => Ok();
}