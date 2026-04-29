using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Services.Authentication.Exceptions;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Core.Services.Authentication.Types;
using Digital.Net.Lib.Configuration;
using Digital.Net.Lib.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Services.Authentication.Filters;

public static class AuthorizationExtensions
{
    /// <summary>
    ///     Handles custom authorization for routes based on provided AuthorizeType.
    /// </summary>
    /// <param name="builder">
    ///     <see cref="RouteHandlerBuilder" />
    /// </param>
    /// <param name="type">
    ///     Applies custom authorization checks based on the specified <see cref="AuthorizeType" />. If multiple types are
    ///     provided,
    ///     each type's authorization is checked sequentially. If any of them is authorized, the route is authorized.
    /// </param>
    /// <example>
    ///     <code>
    ///     var group = app.MapGroup("authentication/user").WithTags("Authentication");
    ///     group.MapPost("route", Action1).RequireAuthentication(AuthorizeType.Jwt);
    /// </code>
    /// </example>
    public static RouteHandlerBuilder RequireAuthentication(this RouteHandlerBuilder builder, AuthorizeType type) =>
        builder.AddEndpointFilter((context, next) => CreateAuthenticationFilter(context, next, type));

    /// <summary>
    ///     Handles custom authorization for route groups based on provided AuthorizeType.
    /// </summary>
    /// <param name="builder">
    ///     <see cref="RouteGroupBuilder" />
    /// </param>
    /// <param name="type">
    ///     Applies custom authorization checks based on the specified <see cref="AuthorizeType" />. If multiple types are
    ///     provided,
    ///     each type's authorization is checked sequentially. If any of them is authorized, the route is authorized.
    /// </param>
    /// <example>
    ///     <code>
    ///     var group = app.MapGroup("authentication/user")
    ///         .RequireAuthentication(AuthorizeType.Jwt)
    ///         .WithTags("Authentication");
    /// </code>
    /// </example>
    public static RouteGroupBuilder RequireAuthentication(this RouteGroupBuilder builder, AuthorizeType type) =>
        builder.AddEndpointFilter((context, next) => CreateAuthenticationFilter(context, next, type));

    /// <summary>
    ///     Enforces admin authorization for routes within a route group.
    /// </summary>
    /// <param name="builder">
    ///     <see cref="RouteGroupBuilder" />
    /// </param>
    public static RouteGroupBuilder RequireAdmin(this RouteGroupBuilder builder) =>
        builder.AddEndpointFilter(CreateAdminAuthorizationFilter);

    /// <summary>
    ///     Enforces admin authorization for routes for a route.
    /// </summary>
    /// <param name="builder">
    ///     <see cref="RouteHandlerBuilder" />
    /// </param>
    /// <returns></returns>
    public static RouteHandlerBuilder RequireAdmin(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter(CreateAdminAuthorizationFilter);

    private static async ValueTask<object?> CreateAdminAuthorizationFilter(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var contextService = context.HttpContext.RequestServices.GetRequiredService<IUserContextService>();
        var user = contextService.GetUser();
        return user.IsAdmin ? await next(context) : Results.StatusCode(403);
    }

    private static async ValueTask<object?> CreateAuthenticationFilter(
        EndpointFilterInvocationContext ctx,
        EndpointFilterDelegate next,
        AuthorizeType type
    )
    {
        var dbCtx = ctx.HttpContext.RequestServices.GetRequiredService<DigitalContext>();
        var jwtService = ctx.HttpContext.RequestServices.GetRequiredService<IJwtService>();
        var config = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var result = new AuthorizationResult();

        if (type.HasFlag(AuthorizeType.ApiKey))
            result.Merge(
                AuthorizeApiKey(
                    dbCtx,
                    ctx.HttpContext.Request.Headers[AuthenticationStaticOptions.ApiKeyHeaderAccessor].FirstOrDefault()
                ));
        if (type.HasFlag(AuthorizeType.Jwt) && !result.IsAuthorized)
            result.Merge(
                jwtService.AuthorizeToken(
                    ctx.HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last()
                ));
        if (type.HasFlag(AuthorizeType.Application) && !result.IsAuthorized)
            result.Merge(
                AuthorizeApplication(
                    config,
                    ctx.HttpContext.Request.Headers[AuthenticationStaticOptions.ApplicationKeyHeaderAccessor]
                        .FirstOrDefault()
                ));

        if (!result.IsAuthorized)
            return Results.StatusCode(401);
        if (result.IsForbidden)
            return Results.StatusCode(403);

        ctx.HttpContext.Items[AuthenticationStaticOptions.ApiContextAuthorizationKey] = result;

        return await next(ctx);
    }

    private static AuthorizationResult AuthorizeApplication(IConfiguration config, string? key)
    {
        var result = new AuthorizationResult();
        var configuredKey = config.Get<string>(AppSettings.ApplicationKeyKey);
        if (string.IsNullOrWhiteSpace(configuredKey) || !string.Equals(key, configuredKey, StringComparison.Ordinal))
            return result.AddError(new InvalidTokenException());
        result.Authorize(Guid.Empty);
        return result;
    }

    private static AuthorizationResult AuthorizeApiKey(DigitalContext dbCtx, string? key)
    {
        var result = new AuthorizationResult();
        if (string.IsNullOrWhiteSpace(key))
            return result.AddError(new TokenNotFoundException());

        var authorization = dbCtx.ApiKeys.FirstOrDefault(k => k.Key == ApiKey.Hash(key));
        if (authorization is null)
            return result.AddError(new InvalidTokenException());

        if (authorization.ExpiredAt is not null && authorization.ExpiredAt < DateTime.UtcNow)
            return result.AddError(new ExpiredTokenException());

        var user = dbCtx.Users.FirstOrDefault(u => u.Id == authorization.UserId && u.IsActive);
        if (user is null)
            return result.AddError(new InvalidTokenException());

        result.Authorize(user.Id);
        return result;
    }
}