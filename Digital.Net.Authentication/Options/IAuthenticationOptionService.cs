using Microsoft.IdentityModel.Tokens;

namespace Digital.Net.Authentication.Options;

public interface IAuthenticationOptionService
{
    public string CookieName { get; }
    public TimeSpan GetMaxLoginAttemptsThreshold();
    public DateTime GetRefreshTokenExpirationDate(DateTime? from = null);
    public DateTime GetBearerTokenExpirationDate(DateTime? from = null);
    public TokenValidationParameters GetTokenParameters();
}