using System.Text;
using Digital.Net.Lib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Digital.Net.Core.Http.Services.Authentication.Options;

public class AuthenticationOptionService(
    IConfiguration configuration,
    IOptions<AuthenticationOptions> options
)
{
    public string CookieName => options.Value.CookieName;

    public TimeSpan GetMaxLoginAttemptsThreshold() =>
        TimeSpan.FromMilliseconds(AuthenticationStaticOptions.MaxLoginAttemptsThreshold);

    public DateTime GetRefreshTokenExpirationDate(DateTime? from = null) =>
        (from ?? DateTime.UtcNow).AddMilliseconds(
            configuration.Get<long?>(CoreSettings.JwtRefreshExpirationKey)
            ?? CoreSettings.DefaultAuthJwtRefreshExpiration
        );

    public DateTime GetBearerTokenExpirationDate(DateTime? from = null) =>
        (from ?? DateTime.UtcNow).AddMilliseconds(
            configuration.Get<long?>(CoreSettings.JwtBearerExpirationKey)
            ?? CoreSettings.DefaultAuthJwtBearerExpiration
        );

    public TokenValidationParameters GetTokenParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        ValidIssuer = options.Value.Issuer,
        ValidAudience = options.Value.Audience,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                configuration.Get<string>(CoreSettings.JwtSecretKey) ?? CoreSettings.DefaultAuthJwtSecret
            )),
        ClockSkew = TimeSpan.Zero
    };
}