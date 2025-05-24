using Digital.Net.Api.Core.Errors;

namespace Digital.Net.Api.Core.Models;

public static class Mapper
{
    /// <summary>
    ///     Maps an instance of type T to an instance of type TM using both values and properties.
    /// </summary>
    /// <param name="item">The instance to map.</param>
    /// <typeparam name="T">The type of the instance to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped instance.</returns>
    public static TM TryMap<T, TM>(T item) where T : class where TM : class =>
        TryCatchUtilities.TryAll(
            () => MapFromConstructor<T, TM>(item),
            () => Map<T, TM>(item)
        ) ?? throw new Exception("Mapping failed. Could not map item to DTO.");

    /// <summary>
    ///     Maps a List of instance of type T to a List of instance of type TM using both values and properties.
    ///     Tries to map using TM's constructor, then using values and properties.
    /// </summary>
    /// <param name="items">The List of instances to map.</param>
    /// <typeparam name="T">The type of the instances to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped List of instances.</returns>
    public static IEnumerable<TM> TryMap<T, TM>(List<T> items) where T : class where TM : class =>
        TryCatchUtilities.TryAll(
            () => MapFromConstructor<T, TM>(items),
            () => Map<T, TM>(items)
        ) ?? throw new Exception("Mapping failed. Could not map items to DTOs.");
    
    /// <summary>
    ///     Maps an instance of type T to an instance of type TM using both values and properties.
    /// </summary>
    /// <param name="instance">The instance to map.</param>
    /// <typeparam name="T">The type of the instance to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped instance.</returns>
    public static TM Map<T, TM>(T instance) where T : class where TM : class
    {
        var target = Activator.CreateInstance<TM>();
        var instanceProperties = instance.GetType().GetProperties();
        var targetProperties = target.GetType().GetProperties();
        foreach (var instanceProperty in instanceProperties)
        {
            var match = targetProperties.FirstOrDefault(
                p =>
                    p.Name == instanceProperty.Name
                    && p.PropertyType == instanceProperty.PropertyType
            );
            match?.SetValue(target, instanceProperty.GetValue(instance));
        }

        return target;
    }

    /// <summary>
    ///     Maps a List of instance of type T to a List of instance of type TM using both values and properties.
    /// </summary>
    /// <param name="instances">The List of instances to map.</param>
    /// <typeparam name="T">The type of the instances to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped List of instances.</returns>
    public static List<TM> Map<T, TM>(List<T> instances) where T : class where TM : class =>
        instances.Select(Map<T, TM>).ToList();

    /// <summary>
    ///     Maps an instance of type T to an instance of type TM using TM's constructor.
    /// </summary>
    /// <param name="instance">The instance to map.</param>
    /// <typeparam name="T">The type of the instance to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped instance.</returns>
    public static TM MapFromConstructor<T, TM>(T instance) where T : class where TM : class
    {
        var constructor = typeof(TM).GetConstructor([instance.GetType()]);
        return constructor is not null
            ? (TM)constructor.Invoke([instance])
            : throw new InvalidOperationException($"Map error: No suitable constructor found for type {typeof(TM).Name}");
    }

    /// <summary>
    ///     Maps a List of instance of type T to a List of instance of type TM using TM's constructor.
    /// </summary>
    /// <param name="instances">The List of instances to map.</param>
    /// <typeparam name="T">The type of the instances to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped List of instances.</returns>
    public static List<TM> MapFromConstructor<T, TM>(List<T> instances) where T : class where TM : class =>
        instances.Select(MapFromConstructor<T, TM>).ToList();
}