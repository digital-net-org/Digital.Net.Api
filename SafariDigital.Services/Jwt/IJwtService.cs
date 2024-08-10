using Safari.Net.Core.Messages;
using SafariDigital.Data.Models.Database;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Services.Jwt;

public interface IJwtService
{
    AuthenticatedUser GetJwtToken();
    JwtToken<AuthenticatedUser> ValidateBearerToken(string? token);
    Result<AuthenticatedUser> ValidateRefreshToken(string? token, string userAgent, string ipAddress);
    Task RegisterToken(string token, User user, string userAgent, string ipAddress);
    Task RevokeToken(string token);
    Task RevokeAllTokens(Guid userId);
    string GenerateBearerToken(User content);
    string GenerateRefreshToken(User content);
}