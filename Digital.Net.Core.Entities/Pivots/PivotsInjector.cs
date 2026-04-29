using System.Reflection;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Core.Entities.Pivots;

public static class PivotsInjector
{
    public static IServiceCollection AddPivotsDependencies(this IServiceCollection services) =>
        services.AddScoped(typeof(PatchDispatcher<>));
    
    /// <summary>
    ///     Scans <paramref name="assemblies" /> for every concrete <see cref="Pivot{TParent,TChild}" />
    ///     class carrying <see cref="PivotResolutionAttribute" />, finds the matching DTO, and registers
    ///     <c>ICrudPatchResolver&lt;TParent&gt; → GenericPivotResolver&lt;TContext,…&gt;</c>
    ///     in the DI container with Scoped lifetime.
    ///     <para>
    ///         Call this once per <typeparamref name="TContext" /> at startup, after the dispatcher
    ///         and validation service are registered.
    ///     </para>
    /// </summary>
    public static IServiceCollection AddPivotsFromAssemblies<TContext>(
        this IServiceCollection services,
        params Assembly[] assemblies
    ) where TContext : DbContext
    {
        if (assemblies.Length == 0)
            throw new ArgumentException("At least one assembly is required.", nameof(assemblies));

        var allTypes = assemblies
            .SelectMany(SafeGetTypes)
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .ToList();

        var dtoTypes = allTypes
            .Where(t => t.GetInterfaces().Any(IsPivotPayloadInterface))
            .ToList();

        foreach (var pivotType in allTypes)
        {
            if (!TryExtractPivotArgs(pivotType, out var parentType, out var childType)) continue;
            if (pivotType.GetCustomAttribute<PivotResolutionAttribute>() is null) continue;

            var dtoType = FindMatchingDto(dtoTypes, pivotType, childType);
            if (dtoType is null)
                throw new InvalidOperationException(
                    $"No DTO implementing IPivotPayload<TSelf, {pivotType.Name}, {childType.Name}> "
                    + $"found in scanned assemblies. Make sure the DTO declares the interface."
                );

            var resolverType =
                typeof(PivotPatchResolver<,,,,>)
                    .MakeGenericType(typeof(TContext), parentType, childType, pivotType, dtoType);
            var resolverInterface =
                typeof(IPivotPatchResolver<>)
                    .MakeGenericType(parentType);

            services.AddScoped(resolverInterface, resolverType);
        }

        return services;
    }

    private static Type? FindMatchingDto(IEnumerable<Type> dtoTypes, Type pivotType, Type childType) =>
        dtoTypes.FirstOrDefault(dto => dto.GetInterfaces().Any(i =>
            IsPivotPayloadInterface(i)
            && i.GenericTypeArguments[1] == pivotType
            && i.GenericTypeArguments[2] == childType));

    private static bool IsPivotPayloadInterface(Type i) =>
        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPivotPayload<,,>);

    private static bool TryExtractPivotArgs(Type pivotType, out Type parentType, out Type childType)
    {
        var current = pivotType.BaseType;
        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Pivot<,>))
            {
                parentType = current.GenericTypeArguments[0];
                childType = current.GenericTypeArguments[1];
                return true;
            }

            current = current.BaseType;
        }

        parentType = null!;
        childType = null!;
        return false;
    }

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