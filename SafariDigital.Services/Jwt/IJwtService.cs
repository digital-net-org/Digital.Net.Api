using SafariDigital.Database.Models.UserTable;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Services.Jwt;

public interface IJwtService
{
    JwtToken<AuthenticatedUser> GetJwtToken();
    JwtToken<AuthenticatedUser> ValidateToken(string? token);
    string GenerateBearerToken(User content);
    string GenerateRefreshToken(User content);
}