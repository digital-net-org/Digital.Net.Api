using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services.Users;

public static class UserServicesInjector
{
    public static IServiceCollection AddDigitalUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
