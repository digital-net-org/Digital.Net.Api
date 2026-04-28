using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Entities.Seeds;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Core.Entities.Test.Seeds;

public class SeederTestSeed(
    ILogger<SeederTestSeed> logger,
    DigitalContext context
) : Seeder<User>(logger, context), ISeed
{
    public static List<User> CreateUsers(string testId) =>
    [
        new()
        {
            Username = $"TestUser_{testId}",
            Password = "TestPassword",
            Login = $"TestLogin_{testId}",
            Email = $"TestEmail_{testId}@email.com"
        },
        new()
        {
            Username = $"TestUser2_{testId}",
            Password = "TestPassword2",
            Login = $"TestLogin2_{testId}",
            Email = $"TestEmail2_{testId}@email.com"
        }
    ];

    public override async Task ApplySeed() => await SeedAsync(CreateUsers("global"));
}
