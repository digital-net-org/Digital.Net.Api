namespace Digital.Net.Core.Http.Services.Authentication.Options;

public static class AuthenticationStaticOptions
{
    public const string ContentClaimType = "Content";
    public const string TokenTypeClaimType = "TokenType";
    public const string ApiContextAuthorizationKey = "AuthorizationResult";
    public const string ApiKeyHeaderAccessor = "DN-Api-Key";
    public const string ApplicationKeyHeaderAccessor = "DN-Application-Key";
    public const int MaxConcurrentSessions = 5;
    public const int MaxLoginAttempts = 3;

    // Deliberately looser than the per-IP counter: prevents lockout DoS
    public const int MaxAccountLoginAttempts = 10;
    public const long MaxLoginAttemptsThreshold = 900000;
    public const int MinLoginDurationMs = 5000;
}