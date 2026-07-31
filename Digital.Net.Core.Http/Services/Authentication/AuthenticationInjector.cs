using Digital.Net.Core.Http.Services.Authentication.Accessor;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Accessors;
using Digital.Net.Lib.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Http.Services.Authentication;

public static class AuthenticationInjector
{
    /// <summary>
    ///     Adds authentication and authorization services to the application.
    /// </summary>
    public static IServiceCollection AddDigitalAuthenticationServices(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var applicationDomain = configuration.GetOrThrow<string>(CoreSettings.ApplicationDomainKey);
        var allowedOrigins = configuration.GetOrThrow<string[]>(CoreSettings.CorsAllowedOriginsKey) ?? [];

        services.Configure<AuthenticationOptions>(opts =>
            opts.CookieSameSite = allowedOrigins.All(origin => IsUnderDomain(origin, applicationDomain))
                ? SameSiteMode.Lax
                : SameSiteMode.None
        );
        services
            .AddScoped<IAuthorizedUserAccessor, UserAccessor>()
            .AddScoped<IUserAccessor>(sp => sp.GetRequiredService<IAuthorizedUserAccessor>())
            .AddScoped<AuthenticationOptionService>()
            .AddScoped<SessionCookieHandler>()
            .AddScoped<AuthEventService>()
            .AddScoped<AuthenticationService>()
            .AddScoped<SessionService>();

        return services;
    }

    private static bool IsUnderDomain(string origin, string domain) =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && (string.Equals(uri.Host, domain, StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase));
}
