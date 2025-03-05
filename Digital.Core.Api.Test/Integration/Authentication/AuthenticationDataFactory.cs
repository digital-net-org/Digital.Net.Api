using Digital.Lib.Net.Authentication.Services;
using Digital.Lib.Net.Core.Random;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Repositories;

namespace Digital.Core.Api.Test.Integration.Authentication;

public static class AuthenticationDataFactory
{
    public const string TestUserPassword = "Testpassword123!";

    public static User BuildTestUser(
        this IRepository<User, DigitalContext> userRepository,
        bool? isActive = null
    )
    {
        var user = new User
        {
            Username = Randomizer.GenerateRandomString(Randomizer.AnyLetter, 20),
            Login = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 12),
            Password = PasswordUtils.HashPassword(TestUserPassword),
            Email = Randomizer.GenerateRandomEmail(),
            IsActive = isActive is null || (bool)isActive,
        };
        userRepository.Create(user);
        userRepository.Save();
        return user;
    }
}
