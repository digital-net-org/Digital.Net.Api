using System.Collections.Concurrent;
using System.Reflection;

namespace Digital.Net.Core.Http.Services.Pagination;

/// <summary>
///     Resolves a user-supplied <c>OrderBy</c> against the sortable (public, instance) properties of an entity.
///     An unknown or blank column falls back to the default sort instead of being forwarded verbatim to
///     System.Linq.Dynamic.Core which would otherwise allow ordering on arbitrary (possibly unindexed) columns.
/// </summary>
public static class OrderByResolver
{
    public const string DefaultColumn = "CreatedAt";

    // Type → canonical property names, keyed case-insensitively (mirrors MutationAuditReader.OrderColumns).
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
}