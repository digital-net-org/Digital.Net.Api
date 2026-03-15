using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Digital.Net.Core.Services.Crud;

public static class CrudServicesInjector
{
    /// <summary>
    ///     Add the Digital Entities services to the service collection.
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddCrudServices(this IServiceCollection services)
    {
        services.TryAddScoped<ICrudValidationService<DigitalContext>, CrudValidationService<DigitalContext>>();
        services.TryAddScoped<ICrudService<Event>, CrudService<DigitalContext, Event>>();
        services.TryAddScoped<ICrudService<User>, CrudService<DigitalContext, User>>();
        return services;
    }
}
