namespace Digital.Net.Api.Core.Random;

public static class Randomizer
{
    private const int DefaultRange = 1000000000;

    public const string CapitalLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string SmallLetters = "abcdefghijklmnopqrstuvwxyz";
    public const string Numbers = "0123456789";
    public const string SpecialCharacters = "!@#$%^&*()";

    public static string AnyNumber => CapitalLetters + SmallLetters;
    public static string AnyLetter => CapitalLetters + SmallLetters;
    public static string AnyLetterOrNumber => CapitalLetters + SmallLetters + Numbers;
    public static string AnyCharacter => CapitalLetters + SmallLetters + Numbers + SpecialCharacters;

    /// <summary>
    ///     Generates a random string of the specified length using the specified characters.
    ///     Default length is 128 and default characters are Randomizer.AnyCharacter.
    /// </summary>
    /// <param name="chars">The characters to use for the random string.</param>
    /// <param name="length">The length of the random string.</param>
    /// <returns>A random string of the specified length using the specified characters.</returns>
    public static string GenerateRandomString(string? chars = null, int? length = null)
    {
        chars ??= AnyCharacter;
        length ??= GenerateRandomInt(1, 128);
        return new string([
            ..Enumerable.Repeat(chars, length.Value)
                .Select(s => s[new System.Random().Next(s.Length)])
        ]);
    }

    /// <summary>
    ///     Generates a random email address.
    /// </summary>
    /// <param name="domain">The domain of the email address.</param>
    /// <param name="topLevelDomain">The top-level domain of the email address.</param>
    /// <returns>A random email address.</returns>
    public static string GenerateRandomEmail(string? domain = null, string? topLevelDomain = null)
    {
        domain ??= GenerateRandomString(SmallLetters, 10);
        topLevelDomain ??= GenerateRandomString(SmallLetters, 3);
        return $"{GenerateRandomString(SmallLetters, 10)}@{domain}.{topLevelDomain}";
    }

    /// <summary>
    ///     Generates a random integer within the specified range.
    ///     Default range is 128 and default characters are Randomizer.AnyCharacter.
    /// </summary>
    /// <param name="range">The range of the random integer.</param>
    /// <returns>A random integer within the specified range.</returns>
    public static int GenerateRandomInt(int range = DefaultRange) => new System.Random().Next(-range, range);

    /// <summary>
    ///     Generates a random integer within the specified range.
    /// </summary>
    /// <param name="rangeMin">The minimum range of the random integer.</param>
    /// <param name="rangeMax">The maximum range of the random integer.</param>
    /// <returns>A random integer within the specified range.</returns>
    public static int GenerateRandomInt(int rangeMin, int rangeMax) => new System.Random().Next(rangeMin, rangeMax);
}