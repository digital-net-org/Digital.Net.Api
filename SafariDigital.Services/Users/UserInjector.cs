using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Services.Users;

public static class UserInjector
{
    public static IServiceCollection AddUserService(this IServiceCollection services) =>
        services.AddScoped<IUserService, UserService>();
}