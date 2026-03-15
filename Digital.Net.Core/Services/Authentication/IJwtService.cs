using Digital.Net.Core.Services.Authentication.Types;

namespace Digital.Net.Core.Services.Authentication;

public interface IJwtService
{
    public Task RevokeTokenAsync(string token);
    public Task RevokeAllTokensAsync(Guid userId);
    public string GenerateBearerToken(Guid userId, string userAgent);
    public string GenerateRefreshToken(Guid userId, string userAgent);
    AuthorizationResult AuthorizeToken(string? key);
}