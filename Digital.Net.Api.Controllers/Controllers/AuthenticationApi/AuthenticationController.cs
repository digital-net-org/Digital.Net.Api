using Digital.Net.Api.Controllers.Controllers.AuthenticationApi.Dto;
using Digital.Net.Api.Core.Extensions.ExceptionUtilities;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Services.Authentication.Attributes;
using Digital.Net.Api.Services.Authentication.Exceptions;
using Digital.Net.Api.Services.Authentication.Options;
using Digital.Net.Api.Services.Authentication.Services.Authentication;
using Digital.Net.Api.Services.HttpContext;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.AuthenticationApi;

[ApiController, Route("authentication/user")]
public class AuthenticationController(
    IAuthenticationService authenticationService,
    IAuthenticationJwtService authenticationJwtService,
    IAuthenticationOptionService authenticationOptionService,
    IUserContextService userContextService,
    IHttpContextService httpContextService
) : ControllerBase
{
    /// <summary>
    ///     Login user with login and password.
    /// </summary>
    /// <returns>Returns a JWT Bearer token in response body and a refresh token in a cookie.</returns>
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
        if (result.Errors.Any(e => e.Reference == new TooManyAttemptsException().GetReference()))
            return StatusCode(429);
        if (result.HasError || string.IsNullOrEmpty(loginRes.Value.bearer))
            return Unauthorized();

        SetCookieToken(loginRes.Value.refresh);
        result.Value = loginRes.Value.bearer;
        return Ok(result);
    }

    /// <summary>
    ///     Refreshes both the JWT Bearer token and the refresh token. If the refresh token will expire soon, it will be renewed.
    ///     Otherwise, only the JWT Bearer token will be refreshed.
    /// </summary>
    /// <returns>Returns a JWT Bearer token in response body and a refresh token in a cookie.</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<Result<string>>> RefreshTokens()
    {
        var result = new Result<string>();
        var userAgent = httpContextService.UserAgent;
        var refreshRes = await authenticationService.RefreshTokensAsync(GetCookieToken(), userAgent);

        result.Merge(refreshRes);
        var (bearerToken, refresh) = refreshRes.Value;

        if (result.HasError || string.IsNullOrEmpty(bearerToken))
            return Unauthorized();
        if (refresh is not null)
            SetCookieToken(refresh);
        
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
    private void SetCookieToken(string refreshToken) =>
        httpContextService.SetResponseCookie(
            refreshToken,
            authenticationOptionService.CookieName,
            authenticationOptionService.GetRefreshTokenExpirationDate()
        );
}