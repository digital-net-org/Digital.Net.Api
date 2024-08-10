using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Safari.Net.Core.Extensions.EnumUtilities;

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

    public static T? GetSection<T>(this IConfiguration configuration, Enum setting) =>
        configuration.GetSection(setting.GetDisplayName()).Get<T>();

    public static T GetSectionOrThrow<T>(this IConfiguration configuration, Enum setting) =>
        configuration.GetSection<T>(setting) ?? throw new Exception($"Setting {setting.GetDisplayName()} not found");
}