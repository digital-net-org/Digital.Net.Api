using Digital.Net.Core.Endpoints.Dto;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Exceptions;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Lib.Exceptions;
using Digital.Net.Lib.Http;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Core.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("authentication/user")
            .WithTags("Authentication")
            .RequireRateLimiting(GlobalLimiter.Policy);

        controller
            .MapPost("login", Login)
            .WithSummary("Login")
            .WithDescription("Login user with login and password.");

        controller
            .MapPost("logout", Logout)
            .RequireAuthentication(AuthorizeType.Jwt)
            .WithSummary("Logout")
            .WithDescription("Logout user's current session.");

        controller
            .MapPost("logout-all", LogoutAll)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey)
            .WithSummary("LogoutAll")
            .WithDescription("Logout all user sessions on all devices.");

        controller
            .MapPost("refresh", RefreshTokens)
            .WithSummary("RefreshTokens")
            .WithDescription("Refreshes JWT and refresh token.");

        return app;
    }

    private static async Task<Results<Ok<Result<string>>, UnauthorizedHttpResult, StatusCodeHttpResult>> Login(
        [FromBody]
        LoginPayload request,
        IAuthenticationService service,
        IAuthenticationOptionService opts,
        HttpContext ctx
    )
    {
        var result = new Result<string>();
        var userAgent = ctx.GetUserAgent() ?? string.Empty;
        var ipAddress = ctx.GetRemoteIpAddress() ?? string.Empty;

        var loginRes = await service.LoginAsync(request.Login, request.Password, userAgent, ipAddress);
        result.Merge(loginRes);

        if (result.Errors.Any(e => e.Reference == new TooManyAttemptsException().GetReference()))
            return TypedResults.StatusCode(429);
        if (result.HasError || string.IsNullOrEmpty(loginRes.Value.bearer))
            return TypedResults.Unauthorized();

        ctx.Response.Cookies.Append(
            opts.CookieName,
            loginRes.Value.refresh,
            BuildCookieOptions(opts.GetRefreshTokenExpirationDate())
        );
        result.Value = loginRes.Value.bearer;
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result<string>>, UnauthorizedHttpResult>> RefreshTokens(
        IAuthenticationService service,
        IAuthenticationOptionService opts,
        HttpContext ctx
    )
    {
        var result = new Result<string>();
        var userAgent = ctx.GetUserAgent() ?? string.Empty;
        var cookie = ctx.Request.Cookies[opts.CookieName];

        if (string.IsNullOrEmpty(cookie))
            return TypedResults.Unauthorized();

        var refreshRes = await service.RefreshTokensAsync(cookie, userAgent);

        result.Merge(refreshRes);
        var (bearerToken, refresh) = refreshRes.Value;

        if (result.HasError || string.IsNullOrEmpty(bearerToken))
            return TypedResults.Unauthorized();

        if (refresh is not null)
            ctx.Response.Cookies.Append(
                opts.CookieName,
                refresh,
                BuildCookieOptions(opts.GetRefreshTokenExpirationDate())
            );

        result.Value = bearerToken;
        return TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> Logout(
        IAuthenticationService service,
        IAuthenticationOptionService opts,
        IUserContextService userCtx,
        HttpContext ctx
    )
    {
        var userAgent = ctx.GetUserAgent() ?? string.Empty;
        var ipAddress = ctx.GetRemoteIpAddress() ?? string.Empty;
        var cookie = ctx.Request.Cookies[opts.CookieName];

        if (string.IsNullOrEmpty(cookie))
            return TypedResults.Unauthorized();

        await service.LogoutAsync(cookie, userCtx.GetUserId(), userAgent, ipAddress);
        ctx.Response.Cookies.Delete(opts.CookieName);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> LogoutAll(
        IAuthenticationService service,
        IAuthenticationOptionService opts,
        HttpContext ctx
    )
    {
        var userAgent = ctx.GetUserAgent() ?? string.Empty;
        var ipAddress = ctx.GetRemoteIpAddress() ?? string.Empty;
        var cookie = ctx.Request.Cookies[opts.CookieName];

        if (string.IsNullOrEmpty(cookie))
            return TypedResults.Unauthorized();

        await service.LogoutAllAsync(cookie, userAgent, ipAddress);
        ctx.Response.Cookies.Delete(opts.CookieName);
        return TypedResults.NoContent();
    }

    private static CookieOptions BuildCookieOptions(DateTime expiration) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = expiration
    };
}