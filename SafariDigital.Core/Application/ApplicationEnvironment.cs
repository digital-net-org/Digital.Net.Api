using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Safari.Net.Core.Extensions.EnumUtilities;

namespace SafariDigital.Core.Application;

public static class ApplicationEnvironment
{
    public static string GetEnvironment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

    public static bool IsTestEnvironment => GetEnvironment == "Test";

    public static WebApplicationBuilder ValidateApplicationSettings(this WebApplicationBuilder builder)
    {
        foreach (var setting in EnumDisplay.GetEnumDisplayNames<EApplicationSetting>())
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