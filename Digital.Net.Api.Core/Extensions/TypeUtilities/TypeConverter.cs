namespace Digital.Net.Api.Core.Extensions.TypeUtilities;

public static class TypeConverter
{
    public static T Convert<T>(string value) where T : notnull
    {
        if (typeof(T) == typeof(string))
            return (T)(object)value;

        if (typeof(T) == typeof(int) && int.TryParse(value, out var intResult))
            return (T)(object)intResult;

        if (typeof(T) == typeof(long) && long.TryParse(value, out var longResult))
            return (T)(object)longResult;

        if (typeof(T) == typeof(double) && double.TryParse(value, out var doubleResult))
            return (T)(object)doubleResult;

        throw new InvalidCastException($"Could not convert '{value}' to {typeof(T)}");
    }
}