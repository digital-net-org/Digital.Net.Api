using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Api.Core.Settings;

public enum OptionAccessor
{
    [Display(Name="JwtRefreshExpiration")]
    JwtRefreshExpiration,
    [Display(Name="JwtBearerExpiration")]
    JwtBearerExpiration,
    [Display(Name="JwtSecret")]
    JwtSecret,
    [Display(Name="FileSystemPath")]
    FileSystemPath,
}