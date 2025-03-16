using Digital.Lib.Net.Authentication.Services;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.ApiKeys;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.Entities.Seeds;

namespace Digital.Core.Api.Seeds;

public class DevelopmentSeed(
    ILogger<DevelopmentSeed> logger,
    IRepository<ApiKey, DigitalContext> apiKeyRepository,
    IRepository<User, DigitalContext> userRepository
) : Seeder<User, DigitalContext>(logger, userRepository), ISeed
{
    public const string DefaultPassword = "Devpassword123!";

    public static string GenerateApiKey(User user) =>
        $"dev_{user.Login.ToLower()}_s12d5fg4h56m56z4ergf561gfj764m4fgsd56fgsj956qierfgd5498746sf8gap9jrp8ez7tazecz079e87u98uo7tyu978az111era98dwckg833574kiumpt"
            [..128];

    public override async Task ApplySeed()
    {
        var result = await SeedAsync(Users);
        if (result.HasError())
            throw new Exception(result.Errors.First().Message);

        foreach (var apiKey in result.Value!.Select(user => new ApiKey(user.Id, GenerateApiKey(user))))
            await apiKeyRepository.CreateAndSaveAsync(apiKey);
    }

    private static List<User> Users =>
    [
        new()
        {
            Username = "user",
            Login = "user",
            Password = PasswordUtils.HashPassword(DefaultPassword),
            Email = "fake-user@fake.com",
            IsActive = true
        }
    ];
}