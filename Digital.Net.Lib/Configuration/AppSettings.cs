using Digital.Net.Lib.Environment;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Lib.Configuration;

public static class AppSettings
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
    public static IConfigurationBuilder AddAppSettings(this IConfigurationBuilder builder)
    {
        builder.AddJsonFile("appsettings.json", true, true);
        if (!AspNetEnv.IsTest)
        {
            builder.AddJsonFile($"appsettings.{AspNetEnv.Get}.json", true, true);
            builder.AddJsonFile("appsettings.local.json", true, true);
        }

        builder.AddEnvironmentVariables();
        return builder;
    }
}