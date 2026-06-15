using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Digital.Net.Lib.Entities.Attributes;

namespace Digital.Net.Lib.Entities.Projection;

public static class EntityProjector
{
    private const int MaxDepth = 32;
    private static readonly ConcurrentDictionary<(Type Source, Type Dto), LambdaExpression> Cache = new();
    private static readonly MethodInfo SelectMethod = EnumerableMethod("Select", 2, 2);
    private static readonly MethodInfo OrderByMethod = EnumerableMethod("OrderBy", 2, 2);
    private static readonly MethodInfo OrderByDescendingMethod = EnumerableMethod("OrderByDescending", 2, 2);
    private static readonly MethodInfo ToListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!;

    /// <summary>
    ///     Project an Entity to a provided DTO.
    /// </summary>
    /// <param name="source">The queryable Entity to project</param>
    /// <typeparam name="TEntity">The database model definition</typeparam>
    /// <typeparam name="TDto">The targeted DTO definition</typeparam>
    public static IQueryable<TDto> ProjectTo<TEntity, TDto>(this IQueryable<TEntity> source)
        where TEntity : class
        where TDto : class
        => source.Select(GetProjection<TEntity, TDto>());

    private static Expression<Func<TEntity, TDto>> GetProjection<TEntity, TDto>()
        where TEntity : class
        where TDto : class
        => (Expression<Func<TEntity, TDto>>)Cache.GetOrAdd(
            (typeof(TEntity), typeof(TDto)),
            static key =>
            {
                var param = Expression.Parameter(key.Source, "e");
                var branch = new HashSet<(Type, Type)> { (key.Source, key.Dto) };
                var body = BuildMemberInit(key.Dto, param, branch);
                return Expression.Lambda(typeof(Func<,>).MakeGenericType(key.Source, key.Dto), body, param);
            });

    private static MemberInitExpression BuildMemberInit(Type dtoType, Expression source, HashSet<(Type, Type)> branch)
    {
        var bindings = new List<MemberBinding>();
        foreach (var dtoProp in dtoType.GetProperties())
        {
            if (!dtoProp.CanWrite) continue;
            if (BindProperty(dtoProp, source, branch) is { } binding) bindings.Add(binding);
        }

        return Expression.MemberInit(Expression.New(dtoType), bindings);
    }

    internal static MemberBinding? BindProperty(PropertyInfo dtoProp, Expression source, HashSet<(Type, Type)> branch)
    {
        var sourceProp = source.Type.GetProperty(dtoProp.Name, BindingFlags.Public | BindingFlags.Instance);
        if (sourceProp is null || !sourceProp.CanRead) return null;
        if (sourceProp.GetCustomAttribute<SecretAttribute>() is not null) return null;

        var access = Expression.Property(source, sourceProp);
        var target = dtoProp.PropertyType;
        var srcType = sourceProp.PropertyType;

        if (target == srcType)
            return Expression.Bind(dtoProp, access);
        if (Nullable.GetUnderlyingType(target) == srcType)
            return Expression.Bind(dtoProp, Expression.Convert(access, target));
        if (!target.IsValueType && target.IsAssignableFrom(srcType))
            return Expression.Bind(dtoProp, access);
        if (TryGetElement(target, out var dtoElem))
            return !IsScalar(dtoElem) && TryGetElement(srcType, out var srcElem) && !IsScalar(srcElem)
                ? BindCollection(dtoProp, access, dtoElem, srcElem, branch)
                : null;
        if (target is { IsClass: true } && !IsScalar(target) && srcType is { IsClass: true } && !IsScalar(srcType))
            return BindReference(dtoProp, access, target, srcType, branch);

        return null;
    }

    private static MemberBinding? BindCollection(
        PropertyInfo dtoProp,
        Expression collection,
        Type dtoElem,
        Type srcElem,
        HashSet<(Type, Type)> branch
    )
    {
        var childInit = BuildGuarded(dtoElem, srcElem, out var childParam, branch);
        if (childInit is null) return null;

        var sequence = collection;
        if (dtoProp.GetCustomAttribute<ProjectOrderByAttribute>() is { } order
            && srcElem.GetProperty(order.PropertyName, BindingFlags.Public | BindingFlags.Instance) is { } sortProp)
        {
            var sortParam = Expression.Parameter(srcElem, "s");
            var sortLambda = Expression.Lambda(Expression.Property(sortParam, sortProp), sortParam);
            var orderMethod =
                (order.Descending ? OrderByDescendingMethod : OrderByMethod)
                .MakeGenericMethod(srcElem, sortProp.PropertyType);

            sequence = Expression.Call(orderMethod, sequence, sortLambda);
        }

        var select = Expression.Call(
            SelectMethod.MakeGenericMethod(srcElem, dtoElem),
            sequence,
            Expression.Lambda(childInit, childParam)
        );

        var toList = Expression.Call(ToListMethod.MakeGenericMethod(dtoElem), select);
        return Expression.Bind(dtoProp, toList);
    }

    private static MemberBinding? BindReference(
        PropertyInfo dtoProp,
        Expression navAccess,
        Type dtoType,
        Type srcType,
        HashSet<(Type, Type)> branch
    )
    {
        var key = (srcType, dtoType);
        if (branch.Count >= MaxDepth || !branch.Add(key)) return null;
        try
        {
            var init = BuildMemberInit(dtoType, navAccess, branch);
            var nullSafe = Expression.Condition(
                Expression.Equal(navAccess, Expression.Constant(null, srcType)),
                Expression.Constant(null, dtoType),
                init);
            return Expression.Bind(dtoProp, nullSafe);
        }
        finally
        {
            branch.Remove(key);
        }
    }

    private static MemberInitExpression? BuildGuarded(
        Type dtoElem,
        Type srcElem,
        out ParameterExpression childParam,
        HashSet<(Type, Type)> branch
    )
    {
        childParam = Expression.Parameter(srcElem, "c");
        var key = (srcElem, dtoElem);
        if (branch.Count >= MaxDepth || !branch.Add(key)) return null;
        try
        {
            return BuildMemberInit(dtoElem, childParam, branch);
        }
        finally
        {
            branch.Remove(key);
        }
    }

    private static bool IsScalar(Type type)
    {
        var u = Nullable.GetUnderlyingType(type) ?? type;
        return u.IsPrimitive || u.IsEnum
                             || u == typeof(string) || u == typeof(decimal) || u == typeof(Guid)
                             || u == typeof(DateTime) || u == typeof(DateTimeOffset) || u == typeof(TimeSpan)
                             || u == typeof(byte[]);
    }

    private static bool TryGetElement(Type type, out Type element)
    {
        element = typeof(object);
        if (type == typeof(string)) return false;
        var enumerable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            ? type
            : Array.Find(type.GetInterfaces(),
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (enumerable is null) return false;
        element = enumerable.GetGenericArguments()[0];
        return true;
    }

    private static MethodInfo EnumerableMethod(string name, int paramCount, int funcArgs) =>
        typeof(Enumerable).GetMethods().Single(m =>
            m.Name == name
            && m.GetParameters().Length == paramCount
            && m.GetParameters()[1].ParameterType.IsGenericType
            && m.GetParameters()[1].ParameterType.GetGenericArguments().Length == funcArgs);
}