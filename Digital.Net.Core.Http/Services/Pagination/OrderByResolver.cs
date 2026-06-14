using System.Collections.Concurrent;
using System.Reflection;

namespace Digital.Net.Core.Http.Services.Pagination;

public static class OrderByResolver
{
    public const string DefaultColumn = "CreatedAt";

    private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> Cache = new();

    public static string Resolve<T>(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return DefaultColumn;
        var columns = Cache.GetOrAdd(typeof(T), static type => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p.Name, StringComparer.OrdinalIgnoreCase));

        return columns.GetValueOrDefault(orderBy, DefaultColumn);
    }

    public static string ResolveOrderClause<T>(string? orderBy, string? order)
    {
        const string tieBreaker = "Id";
        var column = Resolve<T>(orderBy);
        var clause = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase)
            ? $"{column} descending"
            : column;
        return string.Equals(column, tieBreaker, StringComparison.OrdinalIgnoreCase)
            ? clause
            : $"{clause}, {tieBreaker}";
    }
}