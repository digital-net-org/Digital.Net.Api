using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Crud;
using Digital.Net.Api.Entities.Models;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Digital.Net.Api.Entities;

public static class DigitalEntitiesInjector
{
    /// <summary>
    ///     Add the Digital Entities services to the service collection for the specified type.
    ///     This will add the ISeeder, IRepository, and IEntityService to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="T">Entity to register.</typeparam>
    /// <returns></returns>
    public static IServiceCollection AddDigitalEntities<T>(this IServiceCollection services)
        where T : Entity
    {
        services.TryAddScoped<ICrudValidationService<DigitalContext>, CrudValidationService<DigitalContext>>();
        return services
            .AddScoped<IRepository<T, DigitalContext>, Repository<T, DigitalContext>>()
            .AddScoped<ICrudService<T, DigitalContext>, CrudService<T, DigitalContext>>();
    }
}
