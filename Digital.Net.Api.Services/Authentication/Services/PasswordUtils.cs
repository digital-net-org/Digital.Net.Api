using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Services.Authentication.Options;

namespace Digital.Net.Api.Services.Authentication.Services;

public static class PasswordUtils
{
    public static bool VerifyPassword(User user, string password) =>
        BCrypt.Net.BCrypt.Verify(password, user.Password);
    
    public static string HashPassword(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(DefaultAuthenticationOptions.SaltSize);
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }
}