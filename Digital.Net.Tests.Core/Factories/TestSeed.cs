using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Digital.Net.Api.Services.Authentication.Utils;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Models.ApiKeys;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Entities.Seeds;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Tests.Core.Factories;

public class TestSeed(
    ILogger<TestSeed> logger,
    DigitalContext context
) : Seeder<User>(logger, context), ISeed
{
    public const string TestAdminLogin = "Admin";
    public const string TestAdminPassword = "Devpassword123!";

    public const string TestAdminApiKey =
        "administrator_s12d5fg4h56m56z4ergf561gfj764m4fgsd56fgsj956qierfgd5498746sf8gap9jrp8ez7tazecz079e87u98uo7tyu978az111era98dwckg833";

    public override async Task ApplySeed()
    {
        var result = await SeedAsync([
                new User
                {
                    Username = "Administrator",
                    Login = TestAdminLogin,
                    Password = PasswordUtils.HashPassword(TestAdminPassword),
                    Email = "fake-admin@fake.com",
                    IsActive = true,
                    IsAdmin = true
                }
            ]
        );

        await context.ApiKeys.AddAsync(new ApiKey(result.Value!.First().Id, TestAdminApiKey));
        await context.SaveChangesAsync();
    }

    private static List<User> Users =>
    [
        new()
        {
            Username = "Administrator",
            Login = TestAdminLogin,
            Password = PasswordUtils.HashPassword(TestAdminPassword),
            Email = "fake-admin@fake.com",
            IsActive = true,
            IsAdmin = true
        }
    ];
}