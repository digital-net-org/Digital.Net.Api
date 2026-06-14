using Digital.Net.Core.Accessors;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Security;
using Digital.Net.Core.Http.Services.Authentication;
using Digital.Net.Core.Http.Services.Authentication.Exceptions;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Exceptions;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Core.Http.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("authentication/user")
            .WithTags("Authentication")
            .RequireRateLimiting(RateLimiter.Policy);

        controller
            .MapPost("login", Login)
            .WithSummary("Login")
            .WithDescription("Login user with login and password.");

        controller
            .MapGet("is-locked", IsLocked)
            .WithSummary("IsLocked")
            .WithDescription("Check if the client IP has reached the max login attempts.");

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
        AuthenticationService service,
        AuthenticationOptionService opts,
        HttpContext ctx
    )
    {
        var result = new Result<string>();
        var loginRes = await service.LoginAsync(request);
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

    private static async Task<Results<Ok<Result<bool>>, StatusCodeHttpResult>> IsLocked(
        AuthEventService authEvents,
        IOriginAccessor originAccessor
    )
    {
        var result = new Result<bool>();
        var ipAddress = originAccessor.GetOrigin().IpAddress;
        result.Value = ipAddress is not null && await authEvents.HasReachedMaxLoginAttemptsAsync(ipAddress);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result<string>>, UnauthorizedHttpResult>> RefreshTokens(
        AuthenticationService service,
        AuthenticationOptionService opts,
        IOriginAccessor originAccessor,
        HttpContext ctx
    )
    {
        var result = new Result<string>();
        var userAgent = originAccessor.GetOrigin().UserAgent ?? string.Empty;
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

    private static async Task<Results<NoContent, UnauthorizedHttpResult, BadRequest>> Logout(
        AuthenticationService service,
        AuthenticationOptionService opts,
        IUserAccessor userCtx,
        HttpContext ctx
    )
    {
        var cookie = ctx.Request.Cookies[opts.CookieName];
        if (string.IsNullOrEmpty(cookie)) return TypedResults.BadRequest();
        await service.LogoutAsync(cookie);
        ctx.Response.Cookies.Delete(opts.CookieName);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> LogoutAll(
        AuthenticationService service,
        AuthenticationOptionService opts,
        HttpContext ctx
    )
    {
        var cookie = ctx.Request.Cookies[opts.CookieName];
        if (string.IsNullOrEmpty(cookie)) return TypedResults.Unauthorized();
        await service.LogoutAllAsync(cookie);
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