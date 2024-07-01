using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Database.Models.User;
using SafariDigital.Services.Authentication;
using SafariDigital.Services.Authentication.Models;

namespace SafariDigital.Api.Controllers;

[ApiController]
public class AuthController(IAuthenticationService authService) : ControllerBase
{
    [HttpPost("/authentication/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.Login(Request, Response, request.Login, request.Password);
        return result.HasError || result.Value is null ? Unauthorized(result) : Ok(result.Value);
    }

    [HttpGet("/authentication/refresh")]
    public async Task<IActionResult> RefreshTokens()
    {
        var result = await authService.RefreshTokens(Request, Response);
        return result.HasError ? Unauthorized(result) : Ok(result.Value);
    }

    [Authorize(Role = EUserRole.User)]
    [HttpPost("/authentication/logout")]
    public IActionResult Logout()
    {
        authService.Logout(Request, Response);
        return Ok();
    }

    [Authorize(Role = EUserRole.User)]
    [HttpPost("/authentication/logout-all")]
    public IActionResult LogoutAll()
    {
        authService.LogoutAll(Request, Response);
        return Ok();
    }

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