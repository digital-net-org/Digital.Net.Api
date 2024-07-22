namespace Tests.Core.Utils;

public static class RandomUtils
{
    private const string CapitalLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string SmallLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string Numbers = "0123456789";
    private const string SpecialCharacters = "!@#$%^&*()";

    public static string GenerateRandomUsername() =>
        GenerateRandomString(CapitalLetters + SmallLetters + Numbers, 8);

    public static string GenerateRandomPassword() =>
        GenerateRandomString(CapitalLetters + SmallLetters + Numbers + SpecialCharacters, 16);

    public static string GenerateRandomEmail() =>
        $"{GenerateRandomString(SmallLetters, 8)}@fake.com";

    private static string GenerateRandomString(string chars, int length)
    {
        var random = new Random();
        return new string(
            Enumerable.Repeat(chars, length).Select(s => s[new Random().Next(s.Length)]).ToArray()
        );
    }

    public static int GenerateRandomInt() => new Random().Next(-1000, 1000);
}