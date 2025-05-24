namespace Digital.Net.Api.Core.Extensions.StringUtilities;

public static class StringConverter
{

    /// <summary>
    ///     Converts a string value to a snake case string.
    ///     Example: MyString -> my_string
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>The converted string.</returns>
    public static string ToSnakeCase(this string value) =>
        RegularExpressions.PascalCase().Replace(value, "_$1").ToLower();

    /// <summary>
    /// Converts a string value to an upper snake case string.
    ///     Example: MyString -> MY_STRING
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToUpperSnakeCase(this string value) => value.ToSnakeCase().ToUpper();
}