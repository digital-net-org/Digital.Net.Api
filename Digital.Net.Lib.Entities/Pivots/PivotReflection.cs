using System.Reflection;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Lib.Entities.Pivots;

/// <summary>
///     Reflection helpers shared by every component that discovers <see cref="Pivot{TParent,TChild}" />
///     implementations (DI registration, schema generation).
/// </summary>
public static class PivotReflection
{
    public static bool TryExtractArgs(Type pivotType, out Type parentType, out Type childType)
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

    public static IEnumerable<Type> SafeGetTypes(Assembly assembly)
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
