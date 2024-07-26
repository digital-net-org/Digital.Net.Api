using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Services.AuthenticationService;
using SafariDigital.Services.AuthenticationService.Models;

namespace SafariDigital.Api.Controllers.Authentication;

[ApiController]
public class AuthController(IAuthenticationService authService) : ControllerBase
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