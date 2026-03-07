using Digital.Net.Authentication.Models;
using Digital.Net.Authentication.Options;
using Digital.Net.Core.Http;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Entities.Context;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Authentication.Services.Authentication;

public class UserContextService(
    DigitalContext context,
    IHttpContextAccessor httpContextAccessor
) : IUserContextService
{
    public Guid GetUserId()
    {
        var httpContext = httpContextAccessor.GetHttpContext();
        var result =
            httpContext.Items.TryGetValue(AuthenticationStaticOptions.ApiContextAuthorizationKey, out var value) &&
            value is AuthorizationResult typedValue
                ? typedValue
                : null;

        return result?.UserId ?? throw new UnauthorizedAccessException();
    }

    public User GetUser() => context.Users.Find(GetUserId()) ?? throw new UnauthorizedAccessException();
}