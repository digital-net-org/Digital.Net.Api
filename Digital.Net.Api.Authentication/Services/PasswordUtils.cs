using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Entities.Models.Users;

namespace Digital.Net.Api.Authentication.Services;

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