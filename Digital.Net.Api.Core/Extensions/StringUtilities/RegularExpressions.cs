using System.Text.RegularExpressions;

namespace Digital.Net.Api.Core.Extensions.StringUtilities;

/// <summary>
///     Collection of regular expressions for string manipulation.
/// </summary>
public static partial class RegularExpressions
{
    [GeneratedRegex(UsernamePattern)]
    private static partial Regex UsernameRegex();
    public const string UsernamePattern = @"^[a-zA-Z0-9_-]{6,24}$";
    public static Regex Username => UsernameRegex();

    [GeneratedRegex(EmailPattern)]
    private static partial Regex EmailRegex();
    public const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    public static Regex Email => EmailRegex();

    [GeneratedRegex(PasswordPattern)]
    private static partial Regex PasswordRegex();
    public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,128}$";

    public static Regex Password => PasswordRegex();

    [GeneratedRegex("(?<!^)([A-Z])")]
    public static partial Regex PascalCase();

    [GeneratedRegex("(?<!^)([.])")]
    public static partial Regex ObjectName();
}

