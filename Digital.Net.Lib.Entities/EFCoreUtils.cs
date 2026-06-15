namespace Digital.Net.Lib.Entities;

public static class EFCoreUtils
{
    public static string EscapeLike(string value) => value.Replace("\\", @"\\").Replace("%", "\\%").Replace("_", "\\_");
}