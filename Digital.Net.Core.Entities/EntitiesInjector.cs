using Digital.Net.Core.Entities.Pivots;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Entities;

public static class EntitiesInjector
{
    public static IServiceCollection AddEntitiesServices(this IServiceCollection services) =>
        services.AddPivotsDependencies();
}