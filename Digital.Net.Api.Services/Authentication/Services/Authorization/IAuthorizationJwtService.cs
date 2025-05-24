using Digital.Net.Api.Services.Authentication.Models;

namespace Digital.Net.Api.Services.Authentication.Services.Authorization;

public interface IAuthorizationJwtService : IAuthorizationService
{
    public AuthorizationResult AuthorizeRefreshToken(string? token);
}