using Microsoft.IdentityModel.Tokens;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Services.Jwt;

public interface IJwtService
{
    JwtToken<T> ValidateToken<T>(string? token);
    string GenerateBearerToken<T>(T content);
    string GenerateRefreshToken<T>(T content);
    string GetCookieName();
    public long GetBearerTokenExpiration();
    public long GetRefreshTokenExpiration();
    TokenValidationParameters GetTokenParameters();
    SigningCredentials GetSigningSecret();
}