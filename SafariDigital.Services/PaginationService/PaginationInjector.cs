using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Services.PaginationService.Services;

namespace SafariDigital.Services.PaginationService;

public static class PaginationInjector
{
    public static IServiceCollection AddPaginationServices(this IServiceCollection services)
    {
        services
            .AddScoped<IPaginationService<UserPublicModel, UserQuery>, UserPaginationService>();
        return services;
    }
}