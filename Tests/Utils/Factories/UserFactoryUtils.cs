using SafariDigital.Data.Models.Database;
using Safari.Net.Core.Random;
using SafariDigital.Services.Authentication;

namespace Tests.Utils.Factories;

public static class UserFactoryUtils
{
    public static User CreateUser() =>
        new()
        {
            Username = Randomizer.GenerateRandomString(),
            Password = AuthenticationUtils.HashPassword(Randomizer.GenerateRandomString(null, 64)),
            Email = Randomizer.GenerateRandomEmail(),
            Role = EUserRole.User,
            IsActive = true
        };

    public static List<User> CreateManyUsers(int count) =>
        Enumerable.Range(0, count).Select(_ => CreateUser()).ToList();

    public static User Update(this User user, UserPayload payload)
    {
        user.Username = payload.Username ?? user.Username;
        user.Password = payload.Password != null ? AuthenticationUtils.HashPassword(payload.Password) : user.Password;
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
    public EUserRole? Role { get; set; } = null;
    public bool? IsActive { get; set; } = null;
}