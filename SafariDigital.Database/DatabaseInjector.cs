using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Database.Repository;

namespace SafariDigital.Database;

public static class DatabaseInjector
{
    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services
            .AddScoped(typeof(IRepository<>), typeof(Repository<>))
            .AddScoped(typeof(IRepositoryService<>), typeof(RepositoryService<>));
}