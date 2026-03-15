namespace Digital.Net.Lib.String;

public static class StringMatcher
{
    public static bool IsJsonWebToken(this string token) =>
        !string.IsNullOrWhiteSpace(token) && token.Split('.').Length == 3;
}