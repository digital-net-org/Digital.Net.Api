using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Services.Authentication;

public static class AuthenticationInjector
{
    public static IServiceCollection AddAuthenticationService(this IServiceCollection services) =>
        services.AddScoped<IAuthenticationService, AuthenticationService>();
}