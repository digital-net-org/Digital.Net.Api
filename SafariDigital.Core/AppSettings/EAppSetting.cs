using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Core.AppSettings;

public enum EAppSetting
{
    [Display(Name = "AllowedOrigins")] AllowedOrigins,

    [Display(Name = "Jwt:RefreshExpiration")]
    JwtRefreshExpiration,

    [Display(Name = "Jwt:BearerExpiration")]
    JwtBearerExpiration,

    [Display(Name = "Jwt:Secret")] JwtSecret,

    [Display(Name = "Jwt:Issuer")] JwtIssuer,

    [Display(Name = "Jwt:Audience")] JwtAudience,

    [Display(Name = "Jwt:CookieName")] JwtCookieName,

    [Display(Name = "Jwt:MaxTokenAllowed")]
    JwtMaxTokenAllowed,

    [Display(Name = "Security:MaxLoginAttempts")]
    SecurityMaxLoginAttempts,

    [Display(Name = "Security:MaxLoginWindow")]
    SecurityMaxLoginWindow,

    [Display(Name = "Security:MaxRequestAllowed")]
    SecurityMaxRequestAllowed,

    [Display(Name = "Security:MaxRequestWindow")]
    SecurityMaxRequestWindow
}