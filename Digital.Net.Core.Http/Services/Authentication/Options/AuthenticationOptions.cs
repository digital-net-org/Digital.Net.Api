namespace Digital.Net.Core.Http.Services.Authentication.Options;

public class AuthenticationOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string CookieName { get; set; } = string.Empty;
}