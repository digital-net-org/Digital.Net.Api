using Digital.Core.Api.Controllers.AuthenticationApi.Dto;
using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Authentication.Exceptions;
using Digital.Lib.Net.Authentication.Options;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Authentication.Services.Authentication;
using Digital.Lib.Net.Core.Extensions.ExceptionUtilities;
using Digital.Lib.Net.Mvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Core.Api.Controllers.AuthenticationApi;

[ApiController, Route("authentication/user")]
public class AuthenticationController(
    IAuthenticationService authenticationService,
    IAuthenticationJwtService authenticationJwtService,
    IAuthenticationOptionService authenticationOptionService,
    IUserContextService userContextService,
    IHttpContextService httpContextService
) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<Result<string>>> Login([FromBody] LoginPayload request)
    {
        var result = new Result<string>();
        var userAgent = httpContextService.UserAgent;
        var ipAddress = httpContextService.IpAddress;
        var loginRes = await authenticationService.LoginAsync(
            request.Login,
            request.Password,
            userAgent,
            ipAddress
        );

        result.Merge(loginRes);
        var (id, bearerToken) = loginRes.Value;

        if (result.Errors.Any(e => e.Reference == new TooManyAttemptsException().GetReference()))
            return StatusCode(429);
        if (result.HasError || bearerToken is null)
            return Unauthorized();

        SetCookieToken(id, userAgent);
        result.Value = bearerToken;
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<Result<string>>> RefreshTokens()
    {
        var result = new Result<string>();
        var userAgent = httpContextService.UserAgent;
        var refreshRes = await authenticationService.RefreshTokensAsync(GetCookieToken(), userAgent);

        result.Merge(refreshRes);
        var (id, bearerToken) = refreshRes.Value;

        if (result.HasError || bearerToken is null)
            return Unauthorized();

        SetCookieToken(id, userAgent);
        result.Value = bearerToken;
        return Ok(result);
    }

    [HttpPost("logout"), Authorize(AuthorizeType.Jwt)]
    public async Task<IActionResult> Logout()
    {
        var userAgent = httpContextService.UserAgent;
        var ipAddress = httpContextService.IpAddress;
        await authenticationService.LogoutAsync(
            GetCookieToken(),
            userContextService.GetUserId(),
            userAgent,
            ipAddress
        );
        DeleteCookieToken();
        return NoContent();
    }

    [HttpPost("logout-all"), Authorize(AuthorizeType.Any)]
    public async Task<IActionResult> LogoutAll()
   {
        var userAgent = httpContextService.UserAgent;
        var ipAddress = httpContextService.IpAddress;
        await authenticationService.LogoutAllAsync(
            GetCookieToken(),
            userAgent,
            ipAddress
        );
        DeleteCookieToken();
        return NoContent();
    }

    private string? GetCookieToken() =>
        httpContextService.Request.Cookies[authenticationOptionService.CookieName];
    private void DeleteCookieToken() =>
        httpContextService.Response.Cookies.Delete(authenticationOptionService.CookieName);
    private void SetCookieToken(Guid userId, string userAgent) =>
        httpContextService.SetResponseCookie(
            authenticationJwtService.GenerateRefreshToken(userId, userAgent),
            authenticationOptionService.CookieName,
            authenticationOptionService.GetRefreshTokenExpirationDate()
        );
}