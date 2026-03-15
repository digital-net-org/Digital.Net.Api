using Digital.Net.Entities.Context;
using Digital.Net.Entities.Crud;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Entities.Models.Users;
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
        services.TryAddScoped<ICrudValidationService<DigitalContext>, CrudValidationService<DigitalContext>>();
        services.TryAddScoped<ICrudService<Event>, CrudService<DigitalContext, Event>>();
        services.TryAddScoped<ICrudService<User>, CrudService<DigitalContext, User>>();
        return services;
    }
}
