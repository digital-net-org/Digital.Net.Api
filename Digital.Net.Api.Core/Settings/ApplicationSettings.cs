using Digital.Net.Api.Core.Environment;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Api.Core.Settings;

public static class ApplicationSettings
{
    /// <summary>
    ///     Add the following to the configuration builder:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>appsettings.json</description>
    ///         </item>
    ///         <item>
    ///             <description>appsettings.{environment}.json</description>
    ///         </item>
    ///         <item>
    ///             <description>environment variables</description>
    ///         </item>
    ///     </list>
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <returns>The updated configuration builder.</returns>
    public static IConfigurationBuilder AddAppSettings(this IConfigurationBuilder builder) =>
        builder
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{AspNetEnv.Get}.json", true, true)
            .AddEnvironmentVariables();

    public static WebApplicationBuilder ValidateApplicationSettings(this WebApplicationBuilder builder)
    {
        var mandatorySettings = new[]
        {
            ApplicationSettingsAccessor.Domain,
            ApplicationSettingsAccessor.ConnectionString,
        };

        foreach (var setting in mandatorySettings)
        {
            var value = builder.Configuration.GetSection(setting).Value;
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException($"Missing mandatory configuration section: {setting}");
        }

        return builder;
    }
}