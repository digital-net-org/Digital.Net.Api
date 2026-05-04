namespace Digital.Net.Lib.Date;

public static class DateTimeResolver
{
    public static DateTime? MaxUpdatedAt(DateTime? a, DateTime? b)
    {
        if (a is null) return b;
        if (b is null) return a;
        return a > b ? a : b;
    }
}
