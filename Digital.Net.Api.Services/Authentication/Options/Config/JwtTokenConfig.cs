namespace Digital.Net.Api.Services.Authentication.Options.Config;

public class JwtTokenConfig
{
    /// <summary>
    ///     The issuer of the token.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    ///     The audience of the token.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    ///     The name of the cookie storing the refresh token.
    /// </summary>
    public string CookieName { get; set; } = string.Empty;
}
