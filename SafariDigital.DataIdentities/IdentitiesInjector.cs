using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.DataIdentities.Models.User;
using SafariDigital.DataIdentities.Pagination.User;
using SafariDigital.Services.CrudService;
using SafariDigital.Services.PaginationService;

namespace SafariDigital.DataIdentities;

public static class IdentitiesInjector
{
    public static IServiceCollection AddIdentities(this IServiceCollection services)
    {
        services.AddScoped<IPaginationService<UserPublicModel, UserQuery>, UserPaginationService>();
        services
            .AddScoped<ICrudService<User, UserPublicModel, UserQuery>, CrudService<User, UserPublicModel, UserQuery>>();
        return services;
    }
}