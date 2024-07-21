using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafariDigital.Core.Application;

namespace SafariDigital.Services.JwtService;

public static class JwtUtils
{
    public const string ContentClaimType = "Content";

    public static TokenValidationParameters GetTokenParameters(this IConfiguration configuration) =>
        new()
        {
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration.GetSectionOrThrow<string>(EApplicationSetting.JwtIssuer),
            ValidAudience = configuration.GetSectionOrThrow<string>(EApplicationSetting.JwtAudience),
            IssuerSigningKey = GetSecurityKey(configuration.GetSectionOrThrow<string>(EApplicationSetting.JwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

    public static SymmetricSecurityKey GetSecurityKey(string secret) => new(Encoding.ASCII.GetBytes(secret));

    public static string ToString(this SymmetricSecurityKey key) => Encoding.ASCII.GetString(key.Key);

    public static string GetCookieTokenName(this IConfiguration configuration) =>
        configuration.GetSectionOrThrow<string>(EApplicationSetting.JwtCookieName);

    public static long GetBearerTokenExpiration(this IConfiguration configuration) =>
        configuration.GetSectionOrThrow<long>(EApplicationSetting.JwtBearerExpiration);

    public static long GetRefreshTokenExpiration(this IConfiguration configuration) =>
        configuration.GetSectionOrThrow<long>(EApplicationSetting.JwtRefreshExpiration);
}