using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Http.Services.Authentication.Types;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Http.Services.Authentication.Accessor;

public class UserAccessor(
    DigitalContext context,
    IHttpContextAccessor httpContextAccessor
) : IAuthorizedUserAccessor
{
    private HttpContext HttpContext => httpContextAccessor.HttpContext
                                       ?? throw new InvalidOperationException("Http Context is not defined");

    public Guid GetUserId()
    {
        var result =
            HttpContext.Items.TryGetValue(AuthenticationStaticOptions.ApiContextAuthorizationKey, out var value) &&
            value is AuthorizationResult typedValue
                ? typedValue
                : null;

        return result?.UserId ?? throw new UnauthorizedAccessException();
    }

    public Guid? TryGetUserId() =>
        HttpContext.Items.TryGetValue(AuthenticationStaticOptions.ApiContextAuthorizationKey, out var value) &&
        value is AuthorizationResult typedValue
            ? typedValue.UserId
            : null;

    public async Task<User> GetUserAsync(CancellationToken ct = default) =>
        await context.Users.FindAsync([GetUserId()], ct) ?? throw new UnauthorizedAccessException();

    public AuthorizationResult GetAuthorizationResult() =>
        HttpContext.Items.TryGetValue(AuthenticationStaticOptions.ApiContextAuthorizationKey, out var value)
        && value is AuthorizationResult schemeResult
            ? schemeResult
            : throw new UnauthorizedAccessException();

    public void SetAuthorizationResult(AuthorizationResult result) =>
        HttpContext.Items[AuthenticationStaticOptions.ApiContextAuthorizationKey] = result;
}