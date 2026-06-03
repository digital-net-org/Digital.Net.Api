namespace Digital.Net.Core.Entities;

public static class EFCoreUtils
{
    public static string EscapeLike(string value) =>
        value.Replace("\\", @"\\").Replace("%", "\\%").Replace("_", "\\_");
}