using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Http.Services.Authentication.Types;
using Digital.Net.Core.Accessors;
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

    public User GetUser() => context.Users.Find(GetUserId()) ?? throw new UnauthorizedAccessException();
}