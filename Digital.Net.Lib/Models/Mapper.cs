using System.Linq.Expressions;
using System.Reflection;

namespace Digital.Net.Lib.Models;

public static class Mapper
{
    /// <summary>
    ///     Maps an instance of type T to an instance of type TM. Prefers TM's <c>(T)</c> constructor when one exists,
    ///     otherwise copies matching properties. The strategy is decided once per <c>(T, TM)</c> couple (compiled
    ///     delegate) — mapping a row is a direct call, not a reflection <c>Invoke</c>, and real mapping errors are no
    ///     longer swallowed.
    /// </summary>
    /// <param name="item">The instance to map.</param>
    /// <typeparam name="T">The type of the instance to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped instance.</returns>
    public static TM TryMap<T, TM>(T item) where T : class where TM : class =>
        ConstructorMapper<T, TM>.Map is { } map ? map(item) : PropertyMapper<T, TM>.Map(item);

    /// <summary>
    ///     Maps a List of instance of type T to a List of instance of type TM. See <see cref="TryMap{T,TM}(T)" />.
    /// </summary>
    /// <param name="items">The List of instances to map.</param>
    /// <typeparam name="T">The type of the instances to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped List of instances.</returns>
    public static IEnumerable<TM> TryMap<T, TM>(List<T> items) where T : class where TM : class
    {
        var map = ConstructorMapper<T, TM>.Map ?? PropertyMapper<T, TM>.Map;
        return items.Select(map);
    }

    /// <summary>
    ///     Maps an instance of type T to an instance of type TM by copying matching properties.
    /// </summary>
    /// <param name="instance">The instance to map.</param>
    /// <typeparam name="T">The type of the instance to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped instance.</returns>
    public static TM Map<T, TM>(T instance) where T : class where TM : class =>
        PropertyMapper<T, TM>.Map(instance);

    /// <summary>
    ///     Maps a List of instance of type T to a List of instance of type TM by copying matching properties.
    /// </summary>
    /// <param name="instances">The List of instances to map.</param>
    /// <typeparam name="T">The type of the instances to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped List of instances.</returns>
    public static List<TM> Map<T, TM>(List<T> instances) where T : class where TM : class =>
        instances.Select(PropertyMapper<T, TM>.Map).ToList();

    /// <summary>
    ///     Maps an instance of type T to an instance of type TM using TM's <c>(T)</c> constructor.
    /// </summary>
    /// <param name="instance">The instance to map.</param>
    /// <typeparam name="T">The type of the instance to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped instance.</returns>
    public static TM MapFromConstructor<T, TM>(T instance) where T : class where TM : class =>
        ConstructorMapper<T, TM>.Map is { } map
            ? map(instance)
            : throw new InvalidOperationException($"Map error: No suitable constructor found for type {typeof(TM).Name}");

    /// <summary>
    ///     Maps a List of instance of type T to a List of instance of type TM using TM's <c>(T)</c> constructor.
    /// </summary>
    /// <param name="instances">The List of instances to map.</param>
    /// <typeparam name="T">The type of the instances to map.</typeparam>
    /// <typeparam name="TM">The type to map to.</typeparam>
    /// <returns>The mapped List of instances.</returns>
    public static List<TM> MapFromConstructor<T, TM>(List<T> instances) where T : class where TM : class =>
        instances.Select(MapFromConstructor<T, TM>).ToList();

    /// <summary>
    ///     Compiled <c>x => new TM(x)</c> delegate, cached per closed <c>(T, TM)</c> couple. <c>Map</c> is
    ///     <c>null</c> when TM has no constructor accepting a <typeparamref name="T" />. Keyed on the static type
    ///     <typeparamref name="T" /> so EF lazy-loading proxies (a subtype of <typeparamref name="T" />) still map.
    /// </summary>
    private static class ConstructorMapper<T, TM> where T : class where TM : class
    {
        public static readonly Func<T, TM>? Map = Build();

        private static Func<T, TM>? Build()
        {
            var constructor = typeof(TM).GetConstructor([typeof(T)]);
            if (constructor is null)
                return null;
            var param = Expression.Parameter(typeof(T), "x");
            return Expression.Lambda<Func<T, TM>>(Expression.New(constructor, param), param).Compile();
        }
    }

    /// <summary>
    ///     Compiled property-copy delegate (<c>new TM { A = x.A, ... }</c>), cached per closed <c>(T, TM)</c> couple.
    ///     A target property is bound when a source property shares its name and type and is writable.
    /// </summary>
    private static class PropertyMapper<T, TM> where T : class where TM : class
    {
        public static readonly Func<T, TM> Map = Build();

        private static Func<T, TM> Build()
        {
            var param = Expression.Parameter(typeof(T), "x");
            var sourceProperties = typeof(T).GetProperties();
            var bindings = typeof(TM).GetProperties()
                .Where(target => target.CanWrite)
                .Select(target => (target, source: sourceProperties.FirstOrDefault(s =>
                    s.Name == target.Name && s.PropertyType == target.PropertyType && s.CanRead)))
                .Where(x => x.source is not null)
                .Select(x => (MemberBinding)Expression.Bind(x.target, Expression.Property(param, x.source!)));
            var body = Expression.MemberInit(Expression.New(typeof(TM)), bindings);
            return Expression.Lambda<Func<T, TM>>(body, param).Compile();
        }
    }
}
