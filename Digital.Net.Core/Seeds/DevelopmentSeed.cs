using Digital.Net.Core.Services.Authentication.Utils;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Entities.Seeds;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Core.Seeds;

public class DevelopmentSeed(
    ILogger<DevelopmentSeed> logger,
    DigitalContext context
) : Seeder<User>(logger, context), ISeed
{
    private readonly DigitalContext _context = context;
    
    public const string DefaultPassword = "Devpassword123!";

    public static string GenerateApiKey(User user) =>
        $"dev_{user.Login.ToLower()}_s12d5fg4h56m56z4ergf561gfj764m4fgsd56fgsj956qierfgd5498746sf8gap9jrp8ez7tazecz079e87u98uo7tyu978az111era98dwckg833574kiumpt"
            [..128];

    public override async Task ApplySeed()
    {
        var result = await SeedAsync(Users);
        if (result.HasError)
            throw new Exception(result.Errors.First().Message);

        foreach (var apiKey in result.Value!.Select(user => new ApiKey(user.Id, $"dev-{user.Login.ToLower()}", GenerateApiKey(user))))
            await _context.ApiKeys.AddAsync(apiKey);
        await _context.SaveChangesAsync();
    }

    private static List<User> Users =>
    [
        new()
        {
            Username = "Administrator",
            Login = "BenoitSafari",
            Password = PasswordUtils.HashPassword(DefaultPassword),
            Email = "benoitsafari@fake.com",
            IsActive = true,
            IsAdmin = true
        },
        new()
        {
            Username = "user",
            Login = "user",
            Password = PasswordUtils.HashPassword(DefaultPassword),
            Email = "fake-user@fake.com",
            IsActive = true,
            IsAdmin = false
        }
    ];
}