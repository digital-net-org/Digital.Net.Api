using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Services.Users;

public static class UserInjector
{
    public static IServiceCollection AddUserServices(this IServiceCollection services) =>
        services.AddScoped<IUserService, UserService>();
}