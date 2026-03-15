using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Services.Authentication.Filters;

public class ApiDocAuthorizationMiddleware(RequestDelegate next)
{
    private static readonly string[] ProtectedPaths = ["/openapi"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        if (!ProtectedPaths.Any(path.StartsWith))
        {
            await next(context);
            return;
        }

        var apiKey = context.Request.Headers[AuthenticationStaticOptions.ApiKeyHeaderAccessor].FirstOrDefault()
                     ?? context.Request.Query["api-key"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var dbContext = context.RequestServices.GetRequiredService<DigitalContext>();
        var hashedKey = ApiKey.Hash(apiKey);
        var authorization = dbContext.ApiKeys.FirstOrDefault(k => k.Key == hashedKey);

        if (authorization is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (authorization.ExpiredAt is not null && authorization.ExpiredAt < DateTime.UtcNow)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var user = dbContext.Users.FirstOrDefault(u => u.Id == authorization.UserId && u.IsActive);
        if (user is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }
}
