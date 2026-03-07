using Digital.Net.Authentication.Options;
using Digital.Net.Entities.Models.Users;

namespace Digital.Net.Authentication.Services;

public static class PasswordUtils
{
    public static bool VerifyPassword(User user, string password) =>
        BCrypt.Net.BCrypt.Verify(password, user.Password);
    
    public static string HashPassword(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(AuthenticationStaticOptions.SaltSize);
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }
}