using Digital.Net.Api.Core.Extensions.ConfigurationUtilities;
using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Services.Authentication.Options;
using Digital.Net.Api.Services.Authentication.Options.Config;
using Digital.Net.Api.Services.Authentication.Services.Authentication;
using Digital.Net.Api.Services.Authentication.Services.Authorization;
using Digital.Net.Api.Services.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Digital.Net.Api.Services.Authentication;

public static class AuthenticationServicesInjector
{
    public static IServiceCollection AddDigitalAuthenticationServices(this IServiceCollection services)
    {
        var domain = services
            .BuildServiceProvider()
            .GetRequiredService<IConfiguration>()
            .GetOrThrow<string>(ApplicationSettingsAccessor.Domain);
        
        services.Configure<AuthenticationOptions>(opts =>
        {
            opts.SetJwtTokenOptions(new JwtTokenConfig
            {
                Issuer = $"https://{domain}",
                Audience = $"https://{domain}",
                CookieName = $"{domain}_refresh",
            });
            opts.SetApiKeyOptions(new ApiKeyConfig
            {
                HeaderAccessor = $"{domain}_auth"
            });
        });
        services.TryAddScoped<IEventService, EventService>();
        services
            .AddScoped<IUserContextService, UserContextService>()
            .AddScoped<IAuthenticationOptionService, AuthenticationOptionService>()
            .AddScoped<IAuthorizationJwtService, AuthorizationJwtService>()
            .AddScoped<IAuthorizationApiKeyService, AuthorizationApiKeyService>()
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IAuthenticationJwtService, AuthenticationJwtService>();
        
        return services;
    }
}