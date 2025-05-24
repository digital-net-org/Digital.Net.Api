using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Authentication.Models;
using Digital.Net.Api.Services.Authentication.Options;
using Digital.Net.Api.Services.HttpContext;

namespace Digital.Net.Api.Services.Authentication.Services.Authentication;

public class UserContextService(
    IRepository<User, DigitalContext> userRepository,
    IHttpContextService httpContextService
) : IUserContextService
{
    public Guid GetUserId() =>
        httpContextService
            .GetItem<AuthorizationResult>(DefaultAuthenticationOptions.ApiContextAuthorizationKey)
            ?.UserId ?? throw new UnauthorizedAccessException();

    public User GetUser() => userRepository.GetById(GetUserId());
}