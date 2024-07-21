using SafariDigital.Database.Models.UserTable;
using SafariDigital.Services.AuthenticationService;
using Tests.Core.Utils;

namespace Tests.Core.Factories;

public static class UserFactory
{
    public static User CreateUser() =>
        new()
        {
            Username = RandomUtils.GenerateRandomUsername(),
            Password = RandomUtils.GenerateRandomPassword(),
            Email = RandomUtils.GenerateRandomEmail(),
            IsActive = true
        };

    public static User CreateUser(EUserRole? role = null, bool? isActive = null) =>
        new()
        {
            Username = RandomUtils.GenerateRandomUsername(),
            Password = RandomUtils.GenerateRandomPassword(),
            Email = RandomUtils.GenerateRandomEmail(),
            Role = role ?? EUserRole.User,
            IsActive = isActive ?? true
        };

    public static User CreateUserWithHashedPassword(User user) =>
        new()
        {
            Id = user.Id,
            Username = user.Username,
            Password = AuthenticationUtils.HashPassword(user.Password),
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
}