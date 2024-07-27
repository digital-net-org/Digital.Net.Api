using Microsoft.Extensions.DependencyInjection;

namespace SafariDigital.Services.UserService;

public static class UserInjector
{
    public static IServiceCollection AddUserService(this IServiceCollection services) =>
        services.AddScoped<IUserService, UserService>();
}