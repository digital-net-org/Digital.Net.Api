using Digital.Net.Lib.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Digital.Net.Core.Http.Services.Authentication.Options;

public class AuthenticationOptionService(
    IConfiguration configuration,
    IOptions<AuthenticationOptions> options
)
{
    public string CookieName => AuthenticationStaticOptions.SessionCookieName;

    public SameSiteMode CookieSameSite => options.Value.CookieSameSite;

    public TimeSpan GetMaxLoginAttemptsThreshold() =>
        TimeSpan.FromMilliseconds(AuthenticationStaticOptions.MaxLoginAttemptsThreshold);

    /// <summary>How long a session survives without being used.</summary>
    public TimeSpan IdleWindow => TimeSpan.FromMilliseconds(
        configuration.Get<long?>(CoreSettings.SessionIdleExpirationKey)
        ?? CoreSettings.DefaultSessionIdleExpiration
    );

    /// <summary>How long a session lives at most, however active it is.</summary>
    public TimeSpan AbsoluteWindow => TimeSpan.FromMilliseconds(
        configuration.Get<long?>(CoreSettings.SessionAbsoluteExpirationKey)
        ?? CoreSettings.DefaultSessionAbsoluteExpiration
    );

    public TimeSpan RenewalThreshold =>
        TimeSpan.FromMilliseconds(AuthenticationStaticOptions.SessionRenewalThresholdMs);

    public DateTime GetIdleExpirationDate(DateTime? from = null) => (from ?? DateTime.UtcNow).Add(IdleWindow);

    public DateTime GetAbsoluteExpirationDate(DateTime? from = null) => (from ?? DateTime.UtcNow).Add(AbsoluteWindow);
}
