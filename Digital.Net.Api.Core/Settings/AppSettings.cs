using Digital.Net.Api.Core.Environment;
using Digital.Net.Api.Core.Random;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Api.Core.Settings;

public static class AppSettings
{
    public const long DefaultMaxAvatarSize = 2097152;
    public const long DefaultAuthJwtRefreshExpiration = 3600000;
    public const long DefaultAuthJwtBearerExpiration = 300000;
    public const string DefaultFileSystemPath = "/digital_net_storage";
    public static readonly string DefaultAuthJwtSecret = Randomizer.GenerateRandomString(Randomizer.AnyCharacter, 64);

    public const string DomainKey = "Domain";
    public const string CorsAllowedOriginsKey = "CorsAllowedOrigins";
    public const string ConnectionStringKey = "Database:ConnectionString";
    public const string UseSqliteKey = "Database:UseSqlite";
    public const string FileSystemPathKey = "FileSystemPath";
    public const string JwtRefreshExpirationKey = "Auth:JwtRefreshExpiration";
    public const string JwtBearerExpirationKey = "Auth:JwtBearerExpiration";
    public const string JwtSecretKey = "Auth:JwtSecret";

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

    /// <summary>
    ///     Validates the presence of mandatory application configuration settings. Throws an exception if any
    ///     mandatory setting is missing or empty.
    /// </summary>
    /// <param name="builder">The web application builder containing the configuration to validate.</param>
    /// <returns>The updated web application builder after validation.</returns>
    /// <exception cref="NullReferenceException">
    ///     Thrown when any mandatory configuration section is missing or has a null/white-space value.
    /// </exception>
    public static WebApplicationBuilder ValidateApplicationSettings(this WebApplicationBuilder builder)
    {
        var mandatorySettings = new[]
        {
            DomainKey,
            ConnectionStringKey
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