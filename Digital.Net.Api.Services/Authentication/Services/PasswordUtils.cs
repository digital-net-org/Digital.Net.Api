using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Services.Authentication.Options;

namespace Digital.Net.Api.Services.Authentication.Services;

public static class PasswordUtils
{
    /// <summary>
    ///     Verifies the password against the stored hash.
    /// </summary>
    /// <param name="user">
    ///     The user to verify the password against. The password hash is stored in the user object.
    /// </param>
    /// <param name="password">The password to verify.</param>
    /// <returns>True if the password is correct, false otherwise.</returns>
    public static bool VerifyPassword(User user, string password) =>
        BCrypt.Net.BCrypt.Verify(password, user.Password);

    /// <summary>
    ///     Hashes the password using BCrypt.
    /// </summary>
    /// <param name="password"> The password to hash.</param>
    /// <returns></returns>
    public static string HashPassword(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(DefaultAuthenticationOptions.SaltSize);
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }
}