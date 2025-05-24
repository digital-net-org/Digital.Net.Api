using System.Text.RegularExpressions;

namespace Digital.Net.Api.Core.Extensions.EnumUtilities;

public static partial class EnumValues
{
    /// <summary>
    ///     Get the values list of an enum.
    /// </summary>
    /// <typeparam name="T">The enum type to get the values for.</typeparam>
    /// <returns>The values list of the enum.</returns>
    public static IEnumerable<T> GetEnumValues<T>()
        where T : Enum => Enum.GetValues(typeof(T)).Cast<T>();

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex MyRegex();
}