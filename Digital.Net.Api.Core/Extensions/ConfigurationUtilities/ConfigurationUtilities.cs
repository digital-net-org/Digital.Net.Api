using Microsoft.Extensions.Configuration;

namespace Digital.Net.Api.Core.Extensions.ConfigurationUtilities;

public static class ConfigurationUtilities
{
    /// <summary>
    ///     Get configuration entry or throw.
    /// </summary>
    /// <param name="configuration">Configuration object.</param>
    /// <param name="settingPath">Application setting accessor.</param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="Exception"></exception>
    public static T GetOrThrow<T>(this IConfiguration configuration, string settingPath) =>
        configuration.GetSection(settingPath).Get<T>() ?? throw new Exception($"Setting {settingPath} not found");

    /// <summary>
    ///     Get configuration entry or null.
    /// </summary>
    /// <param name="configuration">Configuration object.</param>
    /// <param name="settingPath">Application setting accessor.</param>
    /// <typeparam name="T"></typeparam>
    public static T? Get<T>(this IConfiguration configuration, string settingPath) =>
        configuration.GetSection(settingPath).Get<T>();
}