using Digital.Net.Authentication.Models;
using Digital.Net.Authentication.Options;
using Digital.Net.Authentication.Services.Authentication;
using Digital.Net.Authentication.Services.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Authentication.Filters;

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
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next,
        AuthorizeType type
    )
    {
        var apiKeyService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationApiKeyService>();
        var jwtService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationJwtService>();

        var result = new AuthorizationResult();

        if (type.HasFlag(AuthorizeType.ApiKey))
            result.Merge(apiKeyService.AuthorizeUser(apiKeyService.GetRequestKey()));
        if (type.HasFlag(AuthorizeType.Jwt) && !result.IsAuthorized)
            result.Merge(jwtService.AuthorizeUser(jwtService.GetRequestKey()));
        if (!result.IsAuthorized)
            return Results.StatusCode(401);
        if (result.IsForbidden)
            return Results.StatusCode(403);

        context.HttpContext.Items[AuthenticationStaticOptions.ApiContextAuthorizationKey] = result;

        return await next(context);
    }
}