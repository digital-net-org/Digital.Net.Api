using System.Text.RegularExpressions;

namespace SafariDigital.Core;

public static partial class RegularExpressions
{
    public const string UsernamePattern = @"^[a-zA-Z0-9.'@_-]{6,24}$";

    public const string EmailPattern = @"^[^@]+@[^@]+\.[^@]{2,253}$";

    public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{12,128}$";
    public static Regex Username => UsernameRegex();
    public static Regex Email => EmailRegex();
    public static Regex Password => PasswordRegex();

    [GeneratedRegex(UsernamePattern)]
    private static partial Regex UsernameRegex();

    [GeneratedRegex(EmailPattern)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(PasswordPattern)]
    private static partial Regex PasswordRegex();
}