namespace Digital.Net.Api.Services.Authentication.Options;

public static class DefaultAuthenticationOptions
{
    public const string ContentClaimType = "Content";
    public const string ApiContextAuthorizationKey = "AuthorizationResult";
    public const int SaltSize = 16;
    public const int MaxConcurrentSessions = 5;
    public const int MaxLoginAttempts = 3;
    public const long MaxLoginAttemptsThreshold = 900000;
}