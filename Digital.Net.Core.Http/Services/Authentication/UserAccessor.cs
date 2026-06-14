using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Http.Services.Authentication.Types;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Http.Services.Authentication;

public class UserAccessor(
    DigitalContext context,
    IHttpContextAccessor httpContextAccessor
) : IUserAccessor
{
    public Guid GetUserId()
    {
        var httpContext = httpContextAccessor.HttpContext ??
                          throw new InvalidOperationException("Http Context is not defined");
        var result =
            httpContext.Items.TryGetValue(AuthenticationStaticOptions.ApiContextAuthorizationKey, out var value) &&
            value is AuthorizationResult typedValue
                ? typedValue
                : null;

        return result?.UserId ?? throw new UnauthorizedAccessException();
    }

    public Guid? TryGetUserId()
    {
        if (httpContextAccessor.HttpContext is not { } httpContext) return null;
        return httpContext.Items.TryGetValue(AuthenticationStaticOptions.ApiContextAuthorizationKey, out var value) &&
               value is AuthorizationResult typedValue
            ? typedValue.UserId
            : null;
    }

    public async Task<User> GetUserAsync(CancellationToken ct = default) =>
        await context.Users.FindAsync([GetUserId()], ct) ?? throw new UnauthorizedAccessException();
}