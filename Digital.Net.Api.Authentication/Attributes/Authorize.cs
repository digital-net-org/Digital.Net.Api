using System.Text.Json;
using Digital.Net.Api.Authentication.Filters;
using Digital.Net.Api.Authentication.Models;
using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Authentication.Services.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Authentication.Attributes;

/// <summary>
///     Used to authorize a User to use a controller.
///     The authorization can be done using an API Key, a JWT token, or both.
///     The authorization result is stored in the Application Context.
/// </summary>
/// <param name="type">The type of authorization to use.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuthorizeAttribute(AuthorizeType type) : Attribute, IAuthorizationFilter
{
    private AuthorizeType Type { get; } = type;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var result = new AuthorizationResult();

        if (Type.HasFlag(AuthorizeType.ApiKey))
            result.Merge(AuthorizeApiKey(context));
        if (Type.HasFlag(AuthorizeType.Jwt) && !result.IsAuthorized)
            result.Merge(AuthorizeJwt(context));

        if (result is { IsAuthorized: false })
        {
            context.RejectAuthorization(401);
            return;
        }

        result.Merge(AuthorizeRole(context, result));
        if (result is { IsForbidden: true })
        {
            context.RejectAuthorization(403);
            return;
        }

        context.HttpContext.Items[AuthenticationStaticOptions.ApiContextAuthorizationKey] =
            JsonSerializer.Serialize(result);
    }

    private AuthorizationResult AuthorizeRole(AuthorizationFilterContext context, AuthorizationResult result) => result;

    private AuthorizationResult AuthorizeApiKey(AuthorizationFilterContext context)
    {
        var service = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationApiKeyService>();
        var apiKey = service.GetRequestKey();
        return service.AuthorizeUser(apiKey);
    }

    private AuthorizationResult AuthorizeJwt(AuthorizationFilterContext context)
    {
        var service = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationJwtService>();
        var token = service.GetRequestKey();
        return service.AuthorizeUser(token);
    }
}