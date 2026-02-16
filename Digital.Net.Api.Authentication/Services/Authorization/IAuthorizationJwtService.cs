using Digital.Net.Api.Authentication.Models;

namespace Digital.Net.Api.Authentication.Services.Authorization;

public interface IAuthorizationJwtService : IAuthorizationService
{
    public AuthorizationResult AuthorizeRefreshToken(string? token);
}