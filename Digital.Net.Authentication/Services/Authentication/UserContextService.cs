using Digital.Net.Authentication.Models;
using Digital.Net.Authentication.Options;
using Digital.Net.Core.Http;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Entities.Repositories;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Authentication.Services.Authentication;

public class UserContextService(
    IRepository<User> userRepository,
    IHttpContextAccessor httpContextAccessor
) : IUserContextService
{
    public Guid GetUserId()
    {
        var context = httpContextAccessor.GetHttpContext();
        var result =
            context.Items.TryGetValue(AuthenticationStaticOptions.ApiContextAuthorizationKey, out var value) &&
            value is AuthorizationResult typedValue
                ? typedValue
                : null;

        return result?.UserId ?? throw new UnauthorizedAccessException();
    }

    public User GetUser() => userRepository.GetById(GetUserId()) ?? throw new UnauthorizedAccessException();
}