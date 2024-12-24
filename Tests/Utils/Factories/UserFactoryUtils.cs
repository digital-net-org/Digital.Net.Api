using Digital.Net.Authentication.Services.Security;
using Digital.Net.Core.Random;
using SafariDigital.Data.Models.Users;

namespace Tests.Utils.Factories;

public static class UserFactoryUtils
{
    public static User CreateUser()
    {
        var email = Randomizer.GenerateRandomEmail();
        var user = new User
        {
            Username = Randomizer.GenerateRandomString(),
            Password = HashService.HashPassword(Randomizer.GenerateRandomString(null, 64), 12),
            Email = email,
            Role = UserRole.User,
            IsActive = true,
            Login = email
        };
        return user;
    }

    public static List<User> CreateManyUsers(int count) =>
        Enumerable.Range(0, count).Select(_ => CreateUser()).ToList();

    public static User Update(this User user, UserPayload payload)
    {
        user.Username = payload.Username ?? user.Username;
        user.Password = payload.Password != null ? HashService.HashPassword(payload.Password) : user.Password;
        user.Email = payload.Email ?? user.Email;
        user.Role = payload.Role ?? user.Role;
        user.IsActive = payload.IsActive ?? user.IsActive;
        return user;
    }
}

public class UserPayload
{
    public string? Username { get; set; } = null;
    public string? Password { get; set; } = null;
    public string? Email { get; set; } = null;
    public UserRole? Role { get; set; } = null;
    public bool? IsActive { get; set; } = null;
}