using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Digital.Net.Lib.Entities.Projection;

public static class PivotProjector
{
    private static readonly ConcurrentDictionary<(Type Pivot, Type Dto), LambdaExpression> Cache = new();

    public static Expression<Func<TPivot, TDto>> Project<TPivot, TChild, TDto>()
        where TPivot : class
        where TChild : class
        where TDto : class
        => (Expression<Func<TPivot, TDto>>)Cache.GetOrAdd(
            (typeof(TPivot), typeof(TDto)),
            _ => Build(typeof(TPivot), typeof(TChild), typeof(TDto))
        );

    private static LambdaExpression Build(Type pivotType, Type childType, Type dtoType)
    {
        var param = Expression.Parameter(pivotType, "p");
        var childIdProp = pivotType.GetProperty("ChildId", BindingFlags.Public | BindingFlags.Instance)!;
        var childAccess = Expression.Property(
            param,
            pivotType.GetProperty("Child", BindingFlags.Public | BindingFlags.Instance)!
        );

        var branch = new HashSet<(Type, Type)> { (pivotType, dtoType), (childType, dtoType) };
        var bindings = new List<MemberBinding>();
        foreach (var dtoProp in dtoType.GetProperties())
        {
            if (!dtoProp.CanWrite) continue;
            if (dtoProp.Name == "Id")
            {
                Expression id = Expression.Property(param, childIdProp);
                if (dtoProp.PropertyType != childIdProp.PropertyType)
                    id = Expression.Convert(id, dtoProp.PropertyType);

                bindings.Add(Expression.Bind(dtoProp, id));
                continue;
            }

            var pivotProp = pivotType.GetProperty(dtoProp.Name, BindingFlags.Public | BindingFlags.Instance);
            if (pivotProp is { Name: not "Child" and not "Parent" } && dtoProp.PropertyType == pivotProp.PropertyType)
            {
                bindings.Add(Expression.Bind(dtoProp, Expression.Property(param, pivotProp)));
                continue;
            }

            if (EntityProjector.BindProperty(dtoProp, childAccess, branch) is { } childBinding)
                bindings.Add(childBinding);
        }

        var body = Expression.MemberInit(Expression.New(dtoType), bindings);
        return Expression.Lambda(typeof(Func<,>).MakeGenericType(pivotType, dtoType), body, param);
    }
}