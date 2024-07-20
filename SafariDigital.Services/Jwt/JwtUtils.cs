using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafariDigital.Core.Application;

namespace SafariDigital.Services.Jwt;

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
}