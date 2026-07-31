using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Security;
using Digital.Net.Core.Http.Services.Authentication;
using Digital.Net.Core.Http.Services.Authentication.Exceptions;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Accessors;
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
            .RequireRateLimiting(RateLimiter.Policy)
            .RequireCsrfHeader();

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
            .RequireAuthentication(AuthorizeType.Session)
            .WithSummary("Logout")
            .WithDescription("Logout user's current session.");

        controller
            .MapPost("logout-all", LogoutAll)
            .RequireAuthentication(AuthorizeType.Session | AuthorizeType.ApiKey)
            .WithSummary("LogoutAll")
            .WithDescription("Logout all user sessions on all devices.");

        return app;
    }

    private static async Task<Results<Ok<Result>, UnauthorizedHttpResult, StatusCodeHttpResult>> Login(
        [FromBody]
        LoginPayload request,
        AuthenticationService service,
        AuthenticationOptionService opts,
        SessionCookieHandler cookieHandler,
        HttpContext ctx
    )
    {
        var result = new Result();
        var loginRes = await service.LoginAsync(request);
        result.Merge(loginRes);

        if (result.Errors.Any(e => e.Reference == new TooManyAttemptsException().GetReference()))
            return TypedResults.StatusCode(429);
        if (result.HasError || string.IsNullOrEmpty(loginRes.Value))
            return TypedResults.Unauthorized();

        cookieHandler.Append(loginRes.Value, opts.GetAbsoluteExpirationDate());
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

    private static async Task<NoContent> Logout(
        AuthenticationService service,
        AuthenticationOptionService opts,
        SessionCookieHandler cookieHandler,
        HttpContext ctx
    )
    {
        await service.LogoutAsync(ctx.Request.Cookies[opts.CookieName]!);
        cookieHandler.Delete();
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> LogoutAll(
        AuthenticationService service,
        SessionCookieHandler cookieHandler,
        HttpContext ctx
    )
    {
        await service.LogoutAllAsync();
        cookieHandler.Delete();
        return TypedResults.NoContent();
    }
}