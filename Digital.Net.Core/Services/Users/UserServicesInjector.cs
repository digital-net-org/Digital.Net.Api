using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Services.Users;

public static class UserServicesInjector
{
    public static IServiceCollection AddDigitalUserServices(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        return services;
    }
}
