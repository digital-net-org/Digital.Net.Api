using System.Text;
using Digital.Net.Lib.Configuration;
using Digital.Net.Lib.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Digital.Net.Core.Services.Authentication.Options;

public class AuthenticationOptionService(
    IConfiguration configuration,
    IOptions<AuthenticationOptions> options
) : IAuthenticationOptionService
{
    public string CookieName => options.Value.CookieName;

    public TimeSpan GetMaxLoginAttemptsThreshold() =>
        TimeSpan.FromMilliseconds(AuthenticationStaticOptions.MaxLoginAttemptsThreshold);

    public DateTime GetRefreshTokenExpirationDate(DateTime? from = null) =>
        (from ?? DateTime.UtcNow).AddMilliseconds(
            configuration.Get<long?>(AppSettings.JwtRefreshExpirationKey)
            ?? AppSettings.DefaultAuthJwtRefreshExpiration
        );

    public DateTime GetBearerTokenExpirationDate(DateTime? from = null) =>
        (from ?? DateTime.UtcNow).AddMilliseconds(
            configuration.Get<long?>(AppSettings.JwtBearerExpirationKey)
            ?? AppSettings.DefaultAuthJwtBearerExpiration
        );

    public TokenValidationParameters GetTokenParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        ValidIssuer = options.Value.Issuer,
        ValidAudience = options.Value.Audience,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                configuration.Get<string>(AppSettings.JwtSecretKey) ?? AppSettings.DefaultAuthJwtSecret
            )),
        ClockSkew = TimeSpan.Zero
    };
}