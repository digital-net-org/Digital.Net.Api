using Digital.Net.Api.Authentication.Controllers.Dto;
using Digital.Net.Api.Authentication.Exceptions;
using Digital.Net.Api.Authentication.Filters;
using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Authentication.Services.Authentication;
using Digital.Net.Api.Core.Extensions.ExceptionUtilities;
using Digital.Net.Api.Core.Http;
using Digital.Net.Api.Core.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Authentication.Controllers;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("authentication/user")
            .WithTags("Authentication");

        controller
            .MapPost("login", Login)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Login user with login and password.";
                return Task.CompletedTask;
            });

        controller
            .MapPost("refresh", RefreshTokens)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Refreshes JWT and refresh token.";
                return Task.CompletedTask;
            });

        controller
            .MapPost("logout", Logout)
            .RequireAuthentication(AuthorizeType.Jwt);

        controller
            .MapPost("logout-all", LogoutAll)
            .RequireAuthentication(AuthorizeType.Any);

        return app;
    }

    private static async Task<IResult> Login(
        [FromBody]
        LoginPayload request,
        IAuthenticationService service,
        IAuthenticationOptionService opts,
        IHttpContextAccessor ctx
    )
    {
        var result = new Result<string>();
        var userAgent = ctx.GetUserAgent() ?? string.Empty;
        var ipAddress = ctx.GetRemoteIpAddress() ?? string.Empty;

        var loginRes = await service.LoginAsync(request.Login, request.Password, userAgent, ipAddress);
        result.Merge(loginRes);

        if (result.Errors.Any(e => e.Reference == new TooManyAttemptsException().GetReference()))
            return Results.StatusCode(429);
        if (result.HasError || string.IsNullOrEmpty(loginRes.Value.bearer))
            return Results.Unauthorized();

        ctx.SetResponseCookie(loginRes.Value.refresh, opts.CookieName, opts.GetRefreshTokenExpirationDate());
        result.Value = loginRes.Value.bearer;
        return Results.Ok(result);
    }

    private static async Task<IResult> RefreshTokens(
        IAuthenticationService service,
        IAuthenticationOptionService opts,
        IHttpContextAccessor ctx
    )
    {
        var result = new Result<string>();
        var userAgent = ctx.GetUserAgent() ?? string.Empty;
        var cookie = ctx.GetRequestCookie(opts.CookieName);

        if (string.IsNullOrEmpty(cookie))
            return Results.Unauthorized();

        var refreshRes = await service.RefreshTokensAsync(cookie, userAgent);

        result.Merge(refreshRes);
        var (bearerToken, refresh) = refreshRes.Value;

        if (result.HasError || string.IsNullOrEmpty(bearerToken))
            return Results.Unauthorized();

        if (refresh is not null)
            ctx.SetResponseCookie(refresh, opts.CookieName, opts.GetRefreshTokenExpirationDate());

        result.Value = bearerToken;
        return Results.Ok(result);
    }

    private static async Task<IResult> Logout(
        IAuthenticationService service,
        IAuthenticationOptionService opts,
        IUserContextService userCtx,
        IHttpContextAccessor ctx
    )
    {
        var userAgent = ctx.GetUserAgent() ?? string.Empty;
        var ipAddress = ctx.GetRemoteIpAddress() ?? string.Empty;
        var cookie = ctx.GetRequestCookie(opts.CookieName);

        if (string.IsNullOrEmpty(cookie))
            return Results.Unauthorized();

        await service.LogoutAsync(cookie, userCtx.GetUserId(), userAgent, ipAddress);
        ctx.DeleteCookie(opts.CookieName);
        return Results.NoContent();
    }

    private static async Task<IResult> LogoutAll(
        IAuthenticationService service,
        IAuthenticationOptionService opts,
        IHttpContextAccessor ctx
    )
    {
        var userAgent = ctx.GetUserAgent() ?? string.Empty;
        var ipAddress = ctx.GetRemoteIpAddress() ?? string.Empty;
        var cookie = ctx.GetRequestCookie(opts.CookieName);

        if (string.IsNullOrEmpty(cookie))
            return Results.Unauthorized();

        await service.LogoutAllAsync(cookie, userAgent, ipAddress);
        ctx.DeleteCookie(opts.CookieName);
        return Results.NoContent();
    }
}