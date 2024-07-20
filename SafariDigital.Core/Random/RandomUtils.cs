namespace SafariDigital.Core.Random;

public static class RandomUtils
{
    public static string GenerateRandomSecret() =>
        new(
            Enumerable
                .Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 128)
                .Select(s => s[new System.Random().Next(s.Length)])
                .ToArray()
        );
}