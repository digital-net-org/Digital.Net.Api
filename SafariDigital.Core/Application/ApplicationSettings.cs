using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Core.Enum;

namespace SafariDigital.Core.Application;

public static class ApplicationSettings
{
    public static IConfigurationBuilder AddProjectSettings(this IConfigurationBuilder builder) =>
        builder
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{ApplicationEnvironment.GetEnvironment}.json", true, true)
            .AddEnvironmentVariables();

    public static WebApplicationBuilder AddProjectSettings(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddProjectSettings();
        return builder;
    }

    public static IConfigurationBuilder AddProjectSettings(this IConfigurationBuilder builder, string projectName) =>
        builder.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", projectName)).AddProjectSettings();

    public static T? GetSetting<T>(this IConfiguration configuration, string key)
    {
        if (typeof(T) != typeof(string[]))
            return configuration.GetSection(key).Get<T>();

        var value = configuration.GetSection(key).Value;
        if (string.IsNullOrEmpty(value)) return default;
        var stringArray = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return (T)(object)stringArray;
    }

    public static T? GetSetting<T>(this IConfiguration configuration, System.Enum setting)
    {
        var key = setting.GetDisplayName();
        return configuration.GetSetting<T>(key);
    }

    public static T GetSettingOrThrow<T>(this IConfiguration configuration, string key) =>
        configuration.GetSetting<T>(key) ?? throw new Exception($"Setting {key} not found");

    public static T GetSettingOrThrow<T>(this IConfiguration configuration, System.Enum setting) =>
        configuration.GetSetting<T>(setting) ?? throw new Exception($"Setting {setting.GetDisplayName()} not found");

    public static T? GetSetting<T>(this IServiceCollection services, string key)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        return configuration.GetSetting<T>(key);
    }

    public static T? GetSetting<T>(this IServiceCollection services, System.Enum setting)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        return configuration.GetSetting<T>(setting);
    }

    public static T? GetSettingOrThrow<T>(this IServiceCollection services, string key) =>
        services.GetSetting<T>(key) ?? throw new Exception($"Setting {key} not found");

    public static T? GetSettingOrThrow<T>(this IServiceCollection services, System.Enum setting) =>
        services.GetSetting<T>(setting) ?? throw new Exception($"Setting {setting.GetDisplayName()} not found");
}