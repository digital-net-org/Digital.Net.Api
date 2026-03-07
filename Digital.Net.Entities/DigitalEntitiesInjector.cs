using Digital.Net.Entities.Crud;
using Digital.Net.Entities.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Digital.Net.Entities;

public static class DigitalEntitiesInjector
{
    /// <summary>
    ///     Add the Digital Entities services to the service collection.
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddDigitalEntities(this IServiceCollection services)
    {
        services.TryAddScoped<ICrudValidationService, CrudValidationService>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));
        return services;
    }
}
