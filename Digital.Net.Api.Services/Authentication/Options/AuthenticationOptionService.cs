using System.Text;
using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Services.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Digital.Net.Api.Services.Authentication.Options;

public class AuthenticationOptionService(
    IOptions<AuthenticationOptions> options,
    IOptionsService optionsService
) : IAuthenticationOptionService
{
    public string CookieName => options.Value.JwtTokenConfig.CookieName;
    public string ApiKeyHeaderAccessor => options.Value.ApiKeyConfig.HeaderAccessor;

    public TimeSpan GetMaxLoginAttemptsThreshold() =>
        TimeSpan.FromMilliseconds(DefaultAuthenticationOptions.MaxLoginAttemptsThreshold);

    public DateTime GetRefreshTokenExpirationDate(DateTime? from = null) =>
        (from ?? DateTime.UtcNow).AddMilliseconds(optionsService.Get<long>(OptionAccessor.JwtRefreshExpiration));

    public DateTime GetBearerTokenExpirationDate(DateTime? from = null) =>
        (from ?? DateTime.UtcNow).AddMilliseconds(optionsService.Get<long>(OptionAccessor.JwtBearerExpiration));

    public TokenValidationParameters GetTokenParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        ValidIssuer = options.Value.JwtTokenConfig.Issuer,
        ValidAudience = options.Value.JwtTokenConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(optionsService.Get<string>(OptionAccessor.JwtSecret))
        ),
        ClockSkew = TimeSpan.Zero
    };
}