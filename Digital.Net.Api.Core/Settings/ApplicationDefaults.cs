using Digital.Net.Api.Core.Random;

namespace Digital.Net.Api.Core.Settings;

public static class ApplicationDefaults
{
    public const long MaxAvatarSize = 2097152;
    
    public static readonly List<(string, OptionAccessor, string)> Settings =
    [
        (ApplicationSettingsAccessor.DefaultsJwtBearerExpiration, OptionAccessor.JwtBearerExpiration, "300000"),
        (ApplicationSettingsAccessor.DefaultsJwtRefreshExpiration, OptionAccessor.JwtRefreshExpiration, "1800000"),
        (ApplicationSettingsAccessor.DefaultsJwtSecret, OptionAccessor.JwtSecret, Randomizer.GenerateRandomString(Randomizer.AnyCharacter, 64)),
        (ApplicationSettingsAccessor.DefaultsFileSystemPath, OptionAccessor.FileSystemPath, "/digital_net_storage"),
    ];
}