using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Services.Crud;

public static class DtoEnrichersInjector
{
    /// <summary>
    ///     Scans <paramref name="assemblies" /> for concrete classes implementing
    ///     <see cref="IDtoEnricher{T,TDto}" /> and registers them as Scoped under their respective
    ///     <c>IDtoEnricher&lt;T, TDto&gt;</c> interface. A class that implements the enricher
    ///     interface multiple times (different T/TDto pairs) is registered once per pair.
    /// </summary>
    public static IServiceCollection AddDtoEnrichersFromAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        if (assemblies.Length == 0)
            throw new ArgumentException("At least one assembly is required.", nameof(assemblies));

        foreach (var impl in assemblies.SelectMany(SafeGetTypes).Where(IsConcreteClass))
        foreach (var enricherInterface in impl.GetInterfaces().Where(IsDtoEnricherInterface))
            services.AddScoped(enricherInterface, impl);

        return services;
    }

    private static bool IsConcreteClass(Type type) => type is { IsClass: true, IsAbstract: false };

    private static bool IsDtoEnricherInterface(Type i) =>
        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDtoEnricher<,>);

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }
}