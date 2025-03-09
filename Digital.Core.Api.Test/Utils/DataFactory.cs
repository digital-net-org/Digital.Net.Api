using Digital.Lib.Net.Authentication.Services;
using Digital.Lib.Net.Core.Random;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Repositories;

namespace Digital.Core.Api.Test.Utils;

public static class DataFactory
{
    public const string TestUserPassword = "Testpassword123!";

    public static User BuildTestUser(
        this IRepository<User, DigitalContext> userRepository,
        UserDto? userDto = null
    )
    {
        var user = new User
        {
            Username = userDto?.Username ?? Randomizer.GenerateRandomString(Randomizer.AnyLetter, 20),
            Login = userDto?.Login ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 12),
            Password = PasswordUtils.HashPassword(TestUserPassword),
            Email = userDto?.Email ?? Randomizer.GenerateRandomEmail(),
            IsActive = userDto?.IsActive is null or true,
        };
        userRepository.Create(user);
        userRepository.Save();
        return user;
    }
}
