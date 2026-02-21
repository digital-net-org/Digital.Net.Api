using Digital.Net.Api.Authentication.Models;
using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Authentication.Services.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Authentication.Filters;

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
    ///     group
    ///         .MapPost("route", Action1).RequireAuthentication(AuthorizeType.Jwt);
    ///     group
    ///         .MapPost("route", Action2).RequireAuthentication(AuthorizeType.Any);
    ///     group
    ///         .MapPost("route", Action3).RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);
    /// </code>
    /// </example>
    public static RouteHandlerBuilder RequireAuthentication(this RouteHandlerBuilder builder, AuthorizeType type) =>
        builder.AddEndpointFilter(async (context, next) =>
        {
            var apiKeyService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationApiKeyService>();
            var jwtService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationJwtService>();

            var result = new AuthorizationResult();

            if (type.HasFlag(AuthorizeType.ApiKey))
                result.Merge(apiKeyService.AuthorizeUser(apiKeyService.GetRequestKey()));
            if (type.HasFlag(AuthorizeType.Jwt) && !result.IsAuthorized)
                result.Merge(jwtService.AuthorizeUser(jwtService.GetRequestKey()));
            if (!result.IsAuthorized)
                return Results.Unauthorized();

            result.Merge(AuthorizeRole(result));

            if (result.IsForbidden)
                return Results.Forbid();

            context.HttpContext.Items[AuthenticationStaticOptions.ApiContextAuthorizationKey] = result;

            return await next(context);
        });

    /// <summary>
    ///     Authorize the user based on the role. TODO: Not implemented yet.
    /// </summary>
    /// <returns>
    ///     <see cref="AuthorizationResult" />
    /// </returns>
    private static AuthorizationResult AuthorizeRole(AuthorizationResult result) => result;
}