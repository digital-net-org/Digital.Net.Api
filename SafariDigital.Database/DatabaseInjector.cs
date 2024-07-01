using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Database.Context;
using SafariDigital.Database.Models.User;
using SafariLib.Repositories.Repository;
using SafariLib.Repositories.RepositoryService;

namespace SafariDigital.Database;

public static class DatabaseInjector
{
    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services
            .AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
            .AddScoped(
                typeof(IRepositoryService<User>),
                typeof(RepositoryService<SafariDigitalContext, User>)
            );
}