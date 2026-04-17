namespace Digital.Net.Core.Entities;

public static class EfCoreQuery
{
    public static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}