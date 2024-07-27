using SafariDigital.Database.Models.UserTable;
using SafariDigital.Services.JwtService.Models;

namespace SafariDigital.Services.JwtService;

public interface IJwtService
{
    AuthenticatedUser GetJwtToken();
    JwtToken<AuthenticatedUser> ValidateToken(string? token);
    string GenerateBearerToken(User content);
    string GenerateRefreshToken(User content);
}