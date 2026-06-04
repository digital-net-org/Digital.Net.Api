using Digital.Net.Core.Entities.Models.Users;

namespace Digital.Net.Core.Services.Users;

public static class UserPassword
{
    private const int SaltSize = 16;

    public static bool Verify(User user, string password) =>
        BCrypt.Net.BCrypt.Verify(password, user.Password);

    public static string Hash(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(SaltSize);
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }
}