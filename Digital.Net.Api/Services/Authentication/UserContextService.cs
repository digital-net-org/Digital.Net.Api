using Digital.Net.Api.Services.Authentication.Options;
using Digital.Net.Api.Services.Authentication.Types;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Authentication;

public class UserContextService(
    DigitalContext context,
    IHttpContextAccessor httpContextAccessor
) : IUserContextService
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