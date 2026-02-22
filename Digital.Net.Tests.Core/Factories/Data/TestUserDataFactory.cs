using Digital.Net.Api.Authentication.Services;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Tests.Core.Factories.Data.Records;

namespace Digital.Net.Tests.Core.Factories.Data;

public static class TestUserFactory
{
    public const string TestUserPassword = "Testpassword123!";

    public static User BuildTestUser(
        this IRepository<User> userRepository,
        TestUserPayload? userDto = null
    )
    {
        var user = new User
        {
            Username = userDto?.Username ?? Randomizer.GenerateRandomString(Randomizer.AnyLetter, 20),
            Login = userDto?.Login ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 12),
            Password = PasswordUtils.HashPassword(TestUserPassword),
            Email = userDto?.Email ?? Randomizer.GenerateRandomEmail(),
            IsActive = userDto?.IsActive is null or true
        };
        userRepository.Create(user);
        userRepository.Save();
        return user;
    }
}