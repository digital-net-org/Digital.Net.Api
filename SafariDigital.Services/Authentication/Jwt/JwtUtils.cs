using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafariDigital.Core.Application;

namespace SafariDigital.Services.Authentication.Jwt;

public static class JwtUtils
{
    public const string ContentClaimType = "Content";

    public static TokenValidationParameters GetTokenParameters(this IConfiguration configuration) =>
        new()
        {
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration.GetSection<string>(EApplicationSetting.JwtIssuer),
            ValidAudience = configuration.GetSection<string>(EApplicationSetting.JwtAudience),
            IssuerSigningKey = GetSecurityKey(configuration.GetSection<string>(EApplicationSetting.JwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

    public static SymmetricSecurityKey GetSecurityKey(string secret) => new(Encoding.ASCII.GetBytes(secret));

    public static string GetCookieTokenName(this IConfiguration configuration) =>
        configuration.GetSection<string>(EApplicationSetting.JwtCookieName);

    public static long GetBearerTokenExpiration(this IConfiguration configuration) =>
        configuration.GetSection<long>(EApplicationSetting.JwtBearerExpiration);

    public static long GetRefreshTokenExpiration(this IConfiguration configuration) =>
        configuration.GetSection<long>(EApplicationSetting.JwtRefreshExpiration);
}