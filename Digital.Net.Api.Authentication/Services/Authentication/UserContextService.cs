using Digital.Net.Api.Authentication.Models;
using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Core.Http;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Authentication.Services.Authentication;

public class UserContextService(
    IRepository<User, DigitalContext> userRepository,
    IHttpContextAccessor httpContextAccessor
) : IUserContextService
{
    public Guid GetUserId()
    {
        var context = httpContextAccessor.GetHttpContext();
        var result =
            context.Items.TryGetValue(DefaultAuthenticationOptions.ApiContextAuthorizationKey, out var value) &&
            value is AuthorizationResult typedValue
                ? typedValue
                : null;

        return result?.UserId ?? throw new UnauthorizedAccessException();
    }

    public User GetUser() => userRepository.GetById(GetUserId()) ?? throw new UnauthorizedAccessException();
}