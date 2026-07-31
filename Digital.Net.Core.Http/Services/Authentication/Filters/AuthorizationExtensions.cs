using System.Security.Cryptography;
using System.Text;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Http.Services.Authentication.Accessor;
using Digital.Net.Core.Http.Services.Authentication.Exceptions;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Http.Services.Authentication.Types;
using Digital.Net.Lib.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Http.Services.Authentication.Filters;

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
    ///     group.MapPost("route", Action1).RequireAuthentication(AuthorizeType.Session);
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
    ///         .RequireAuthentication(AuthorizeType.Session)
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
        var contextService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizedUserAccessor>();
        var user = await contextService.GetUserAsync(context.HttpContext.RequestAborted);
        return user.IsAdmin ? await next(context) : Results.StatusCode(403);
    }

    /// <summary>
    ///     Rejects mutating requests that lack the custom header, whatever the authorization scheme. Meant for
    ///     public routes, which have no authentication filter to carry the check.
    /// </summary>
    public static RouteGroupBuilder RequireCsrfHeader(this RouteGroupBuilder builder) =>
        builder.AddEndpointFilter(CreateCsrfFilter);

    private static async ValueTask<object?> CreateCsrfFilter(
        EndpointFilterInvocationContext ctx,
        EndpointFilterDelegate next
    ) =>
        IsSafeMethod(ctx.HttpContext.Request.Method) || ctx.HttpContext.HasCsrfHeader()
            ? await next(ctx)
            : Results.StatusCode(403);

    private static async ValueTask<object?> CreateAuthenticationFilter(
        EndpointFilterInvocationContext ctx,
        EndpointFilterDelegate next,
        AuthorizeType type
    )
    {
        var dbCtx = ctx.HttpContext.RequestServices.GetRequiredService<DigitalContext>();
        var config = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var authOptions = ctx.HttpContext.RequestServices.GetRequiredService<AuthenticationOptionService>();
        var userAccessor = ctx.HttpContext.RequestServices.GetRequiredService<IAuthorizedUserAccessor>();
        var result = new AuthorizationResult();

        if (type.HasFlag(AuthorizeType.ApiKey))
        {
            result.Merge(
                await AuthorizeApiKeyAsync(
                    dbCtx,
                    ctx.HttpContext.Request.Headers[AuthenticationStaticOptions.ApiKeyHeaderAccessor].FirstOrDefault(),
                    ctx.HttpContext.RequestAborted
                ));
        }

        if (type.HasFlag(AuthorizeType.Session) && !result.IsAuthorized)
        {
            var sessionService = ctx.HttpContext.RequestServices.GetRequiredService<SessionService>();
            result.Merge(
                await sessionService.AuthorizeAsync(
                    ctx.HttpContext.Request.Cookies[authOptions.CookieName],
                    ctx.HttpContext.RequestAborted
                ));
        }
        
        if (type.HasFlag(AuthorizeType.Application) && !result.IsAuthorized)
        {
            result.Merge(
                AuthorizeApplication(
                    config,
                    ctx.HttpContext.Request.Headers[AuthenticationStaticOptions.ApplicationKeyHeaderAccessor]
                        .FirstOrDefault()
                ));
        }
        
        if (!result.IsAuthorized)
        {
            ctx.HttpContext.RequestServices.GetRequiredService<SessionCookieHandler>().Delete();
            return Results.StatusCode(401);
        }

        if (
            result.IsForbidden || (
                result.Scheme is AuthorizeType.Session
                && !IsSafeMethod(ctx.HttpContext.Request.Method)
                && !ctx.HttpContext.HasCsrfHeader()
            )
        )
        {
            return Results.StatusCode(403);
        }

        userAccessor.SetAuthorizationResult(result);
        return await next(ctx);
    }

    private static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method);

    /// <summary>
    ///     Check if the CSRF Header is present in the request.
    /// </summary>
    /// <remarks>
    ///     Presence is the whole check: a cross-site request cannot set a custom header without a preflight that
    ///     the CORS policy would refuse. The value isn't a secret and does not add any cryptographic strength.
    ///     See the OWASP recommendations for further information:
    ///     https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html#employing-custom-request-headers-for-ajaxapi
    /// </remarks>
    /// <param name="ctx">The current request context.</param>
    public static bool HasCsrfHeader(this HttpContext ctx) =>
        !string.IsNullOrEmpty(ctx.Request.Headers[AuthenticationStaticOptions.CsrfHeaderAccessor]);

    private static AuthorizationResult AuthorizeApplication(IConfiguration config, string? key)
    {
        var result = new AuthorizationResult();
        var configuredKey = config.Get<string>(CoreSettings.ApplicationKeyKey);
        if (string.IsNullOrWhiteSpace(configuredKey) || string.IsNullOrEmpty(key)) 
            return result.AddError(new InvalidTokenException());

        var isSameKey = CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(key)),
            SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey))
        );
        if (!isSameKey)
            return result.AddError(new InvalidTokenException());
        
        result.Authorize(Guid.Empty);
        result.Scheme = AuthorizeType.Application;
        return result;
    }
    
    private static async Task<AuthorizationResult> AuthorizeApiKeyAsync(
        DigitalContext dbCtx,
        string? key,
        CancellationToken ct
    )
    {
        var result = new AuthorizationResult();
        if (string.IsNullOrWhiteSpace(key))
            return result.AddError(new TokenNotFoundException());

        var authorization = await dbCtx.ApiKeys.FirstOrDefaultAsync(k => k.Key == ApiKey.Hash(key), ct);
        if (authorization is null)
            return result.AddError(new InvalidTokenException());

        if (authorization.ExpiredAt is not null && authorization.ExpiredAt < DateTime.UtcNow)
            return result.AddError(new ExpiredTokenException());

        var user = await dbCtx.Users.FirstOrDefaultAsync(u => u.Id == authorization.UserId && u.IsActive, ct);
        if (user is null)
            return result.AddError(new InvalidTokenException());

        result.Authorize(user.Id);
        result.Scheme = AuthorizeType.ApiKey;
        return result;
    }
}