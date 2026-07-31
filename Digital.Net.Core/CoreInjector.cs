using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Services.ApiKeys;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Users;
using Digital.Net.Lib.Accessors;
using Digital.Net.Lib.Configuration;
using Digital.Net.Lib.Entities.Bootstrap;
using Digital.Net.Lib.Validation;
using Microsoft.Extensions.Hosting;

namespace Digital.Net.Core;

public static class CoreInjector
{
    /// <summary>
    ///     Registers the Digital.Net core business layer (DbContext, migrations, domain services).
    ///     HTTP wiring lives in Digital.Net.Core.Http (AddDigitalNetCoreHttp / UseDigitalNetCore).
    /// </summary>
    public static TBuilder AddDigitalNetCore<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Configuration.AddAppSettings();
        builder.ValidateApplicationSettings();
        builder.AddDatabaseContext<DigitalContext>(
            builder.Configuration.GetOrThrow<string>(CoreSettings.ConnectionStringKey));

        builder.Services
            .AddDigitalApiKeyServices()
            .AddDigitalUserServices()
            .AddDigitalDocumentServices();

        builder.Services
            .RequireContract<IOriginAccessor>(nameof(AddDigitalNetCore))
            .RequireContract<IUserAccessor>(nameof(AddDigitalNetCore));

        return builder;
    }

    private static IHostApplicationBuilder ValidateApplicationSettings(this IHostApplicationBuilder builder)
    {
        var mandatorySettings = new[]
        {
            CoreSettings.ApplicationDomainKey,
            CoreSettings.ApplicationKeyKey,
            CoreSettings.ConnectionStringKey,
        };

        foreach (var setting in mandatorySettings)
        {
            var value = builder.Configuration.GetSection(setting).Value;
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException($"Missing mandatory configuration section: {setting}");
        }

        var allowedOrigins = builder.Configuration.Get<string[]>(CoreSettings.CorsAllowedOriginsKey) ?? [];
        if (allowedOrigins.Length == 0 || allowedOrigins.Any(string.IsNullOrWhiteSpace))
            throw new NullReferenceException(
                $"Missing mandatory configuration section: {CoreSettings.CorsAllowedOriginsKey}"
            );

        var idle = builder.Configuration.Get<long?>(CoreSettings.SessionIdleExpirationKey)
                   ?? CoreSettings.DefaultSessionIdleExpiration;
        var absolute = builder.Configuration.Get<long?>(CoreSettings.SessionAbsoluteExpirationKey)
                       ?? CoreSettings.DefaultSessionAbsoluteExpiration;

        if (idle > absolute)
            throw new InvalidOperationException(
                $"{CoreSettings.SessionIdleExpirationKey} must not exceed {CoreSettings.SessionAbsoluteExpirationKey}."
            );

        return builder;
    }
}