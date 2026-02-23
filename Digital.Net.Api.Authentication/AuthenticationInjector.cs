using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Authentication.Services.AuthContext;
using Digital.Net.Api.Authentication.Services.Authentication;
using Digital.Net.Api.Authentication.Services.Authorization;
using Digital.Net.Api.Core.Configuration;
using Digital.Net.Api.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Authentication;

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
            .GetOrThrow<string>(AppSettings.DomainKey);

        services.Configure<AuthenticationOptions>(opts =>
        {
            opts.Issuer = $"https://{domain}";
            opts.Audience = $"https://{domain}";
            opts.CookieName = $"{domain}_refresh";
        });
        services
            .AddScoped<IUserContextService, UserContextService>()
            .AddScoped<IAuthContextService, AuthContextService>()
            .AddScoped<IAuthenticationOptionService, AuthenticationOptionService>()
            .AddScoped<IAuthorizationJwtService, AuthorizationJwtService>()
            .AddScoped<IAuthorizationApiKeyService, AuthorizationApiKeyService>()
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IAuthenticationJwtService, AuthenticationJwtService>();

        return services;
    }
}