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
        services.TryAddScoped(typeof(CrudService<,>));
        return services;
    }
}
