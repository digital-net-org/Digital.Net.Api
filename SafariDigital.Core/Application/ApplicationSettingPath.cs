using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Core.Application;

public enum ApplicationSettingPath
{
    [Display(Name = "CorsAllowedOrigins")]
    CorsAllowedOrigins,

    [Display(Name = "FileSystem:Path")]
    FileSystemPath,

    [Display(Name = "FileSystem:maxAvatarSize")]
    FileSystemMaxAvatarSize,

    [Display(Name = "Jwt:RefreshExpiration")]
    JwtRefreshExpiration,

    [Display(Name = "Jwt:BearerExpiration")]
    JwtBearerExpiration,

    [Display(Name = "Jwt:Secret")]
    JwtSecret,

    [Display(Name = "Jwt:Issuer")]
    JwtIssuer,

    [Display(Name = "Jwt:Audience")]
    JwtAudience,

    [Display(Name = "Jwt:CookieName")]
    JwtCookieName,

    [Display(Name = "Jwt:SaltSize")]
    JwtSaltSize,

    [Display(Name = "Jwt:ConcurrentSessions")]
    JwtConcurrentSessions,

    [Display(Name = "Jwt:MaxAttempts")]
    JwtMaxAttempts
}