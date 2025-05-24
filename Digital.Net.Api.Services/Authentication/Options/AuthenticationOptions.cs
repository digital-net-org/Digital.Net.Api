using Digital.Net.Api.Services.Authentication.Options.Config;

namespace Digital.Net.Api.Services.Authentication.Options;

public class AuthenticationOptions
{
    public ApiKeyConfig ApiKeyConfig { get; private set; } = new();

    /// <summary>
    ///     Set Api keys options.
    /// </summary>
    public AuthenticationOptions SetApiKeyOptions(ApiKeyConfig apiKeyConfig)
    {
        ApiKeyConfig = apiKeyConfig;
        return this;
    }

    public JwtTokenConfig JwtTokenConfig { get; private set; } = new();

    /// <summary>
    ///     Set the JWT token configuration options.
    /// </summary>
    public AuthenticationOptions SetJwtTokenOptions(JwtTokenConfig config)
    {
        JwtTokenConfig = config;
        return this;
    }
}