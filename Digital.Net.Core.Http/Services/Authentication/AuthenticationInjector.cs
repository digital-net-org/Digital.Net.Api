using Digital.Net.Core.Accessors;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Lib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Http.Services.Authentication;

public static class AuthenticationInjector
{
    /// <summary>
    ///     Adds authentication and authorization services to the application.
    /// </summary>
    /// <remarks>
    ///     The authentication system is based on JWT tokens and API Keys. The JWT configuration values are resolved
    ///     from the application's configuration (see the application README.md).
    /// </remarks>
    public static IServiceCollection AddDigitalAuthenticationServices(this IServiceCollection services)
    {
        var domain = services
            .BuildServiceProvider()
            .GetRequiredService<IConfiguration>()
            .GetOrThrow<string>(CoreSettings.DomainKey);

        services.Configure<AuthenticationOptions>(opts =>
        {
            opts.Issuer = $"https://{domain}";
            opts.Audience = $"https://{domain}";
            opts.CookieName = $"{domain}_refresh";
        });
        services
            .AddScoped<IUserAccessor, UserAccessor>()
            .AddScoped<AuthenticationOptionService>()
            .AddScoped<AuthEventService>()
            .AddScoped<AuthenticationService>()
            .AddScoped<JwtService>();

        return services;
    }
}