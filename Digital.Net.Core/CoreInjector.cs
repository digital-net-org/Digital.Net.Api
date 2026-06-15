using Digital.Net.Core.Accessors;
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
            CoreSettings.DomainKey,
            CoreSettings.ConnectionStringKey,
            CoreSettings.JwtSecretKey,
            CoreSettings.ApplicationKeyKey
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