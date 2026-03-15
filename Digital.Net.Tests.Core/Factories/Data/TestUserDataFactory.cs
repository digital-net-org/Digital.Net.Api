using Digital.Net.Core.Services.Authentication.Utils;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core.Factories.Data.Records;

namespace Digital.Net.Tests.Core.Factories.Data;

public static class TestUserFactory
{
    public const string TestUserPassword = "Testpassword123!";

    public static User BuildTestUser(
        this DigitalContext context,
        TestUserPayload? userDto = null
    )
    {
        var user = new User
        {
            Username = userDto?.Username ?? Randomizer.GenerateRandomString(Randomizer.AnyLetter, 20),
            Login = userDto?.Login ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 12),
            Password = PasswordUtils.HashPassword(TestUserPassword),
            Email = userDto?.Email ?? Randomizer.GenerateRandomEmail(),
            IsActive = userDto?.IsActive ?? true,
            IsAdmin = userDto?.IsAdmin ?? false
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }
}