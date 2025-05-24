using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Authentication.Services;

namespace Digital.Net.Api.TestUtilities.Data.Factories;

public static class TestUserFactory
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
