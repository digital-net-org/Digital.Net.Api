using SafariDigital.Database.Models.User;

namespace SafariDigital.Services.Authentication;

public static class AuthenticationUtils
{
    public const string DefaultUserAgent = "no_user_agent_found";
    public const string DefaultIpAddress = "no_ip_address_found";

    public static bool VerifyPassword(User user, string password) =>
        BCrypt.Net.BCrypt.Verify(password, user.Password);

    public static string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(10));
}