using System.Text.RegularExpressions;

namespace SafariDigital.Core;

public static partial class RegularExpressions
{
    public static Regex GetUsernameRegex() => UsernameRegex();
    public static Regex GetEmailRegex() => EmailRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9.'@_-]{6,24}$")]
    private static partial Regex UsernameRegex();

    [GeneratedRegex(@"^[^@]+@[^@]+\.[^@]{2,253}$")]
    private static partial Regex EmailRegex();
}