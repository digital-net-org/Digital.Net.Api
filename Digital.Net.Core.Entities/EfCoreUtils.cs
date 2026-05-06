namespace Digital.Net.Core.Entities;

public static class EfCoreUtils
{
    public static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    /// <summary>
    ///     Strips EF Core proxy suffix so the dictionary key matches the logical
    ///     type name (e.g. <c>"article"</c>, not <c>"articleproxy"</c>).
    /// </summary>
    public static Type GetCanonicalType(object instance)
    {
        var type = instance.GetType();
        return type.Name.EndsWith("Proxy") && type.BaseType is not null ? type.BaseType : type;
    }
}