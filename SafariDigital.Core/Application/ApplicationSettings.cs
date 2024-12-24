using Digital.Net.Core.Extensions.EnumUtilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace SafariDigital.Core.Application;

public static class ApplicationSettings
{
    public static T GetSection<T>(this IConfiguration configuration, Enum setting) =>
        configuration.GetSection(setting.GetDisplayName()).Get<T>()
        ?? throw new Exception($"Setting {setting.GetDisplayName()} not found");

    public static WebApplicationBuilder ValidateApplicationSettings(this WebApplicationBuilder builder)
    {
        foreach (var setting in EnumDisplay.GetEnumDisplayNames<ApplicationSettingPath>())
        {
            var section = builder.Configuration.GetSection(setting);
            if (!section.Exists())
                throw new Exception($"Application setting {setting} is not set.");

            var arrValue = section.Get<string[]>();
            if ((arrValue is not null && arrValue.Length == 0) ||
                (arrValue is null && string.IsNullOrEmpty(section.Value)))
                throw new Exception($"Application setting {setting} is empty.");
        }

        return builder;
    }
}