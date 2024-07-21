using SafariDigital.Database.Models.User;

namespace SafariDigital.Services.Authentication;

public static class AuthenticationUtils
{
    public static bool VerifyPassword(User user, string password) =>
        BCrypt.Net.BCrypt.Verify(password, user.Password);

    public static string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(10));
}