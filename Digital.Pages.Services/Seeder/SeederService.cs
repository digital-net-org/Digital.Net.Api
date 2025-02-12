using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.Entities.Services;
using Digital.Pages.Data.Models.ApiKeys;
using Digital.Pages.Data.Models.Users;
using Digital.Pages.Services.Seeder.Seeds;

namespace Digital.Pages.Services.Seeder;

public class SeederService(
    ISeeder<User> userSeeder,
    IRepository<ApiKey> apiKeyRepository
) : ISeederService
{
    public async Task SeedDevelopmentDataAsync()
    {
        var result = await userSeeder.SeedAsync(DevelopmentSeed.Users);
        if (result.HasError())
            throw new Exception(result.Errors.First().Message);

        foreach (var apiKey in result.Value!.Select(
                     user => new ApiKey(
                         user.Id,
                         $"dev_{user.Login.ToLower()}_s12d5fg4h56m56z4ergf561gfj764m4fgsd56fgsj956qierfgd5498746sf8gap9jrp8ez7tazecz079e87u98uo7tyu978az111era98dwckg833574kiumpt"
                             [..128]
                     )
                 )
                )
        {
            await apiKeyRepository.CreateAsync(apiKey);
            await apiKeyRepository.SaveAsync();
        }
    }
}