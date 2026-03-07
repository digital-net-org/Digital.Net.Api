using Digital.Net.Authentication.Models;

namespace Digital.Net.Authentication.Services.Authorization;

public interface IAuthorizationJwtService : IAuthorizationService
{
    public AuthorizationResult AuthorizeRefreshToken(string? token);
}