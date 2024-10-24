using Microsoft.Extensions.Logging;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Repositories;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Seeds;

namespace SafariDigital.Data.Models;

public class Seeder(
    ILogger<Seeder> logger,
    IRepository<ApiKey> apiKeyRepository,
    IRepository<User> userRepository
) : ISeeder
{
    public async Task<Result> SeedDevelopmentData()
    {
        var result = new Result();
        foreach (var entity in DevSeed.ApiKeys.Select(apiKey => new ApiKey { Key = apiKey }))
            try
            {
                var existingKey = apiKeyRepository.Get(apiKey => apiKey.Key == entity.Key);
                if (existingKey.Any())
                {
                    logger.LogInformation($"ApiKey {entity.Key} already exists, skipping.");
                    continue;
                }

                await apiKeyRepository.CreateAsync(entity);
                await apiKeyRepository.SaveAsync();
            }
            catch (Exception e)
            {
                result.AddError(e);
            }

        foreach (var entity in DevSeed.Users)
            try
            {
                var existingUser = userRepository.Get(user => user.Username == entity.Username);
                if (existingUser.Any())
                {
                    logger.LogInformation($"User {entity.Username} already exists, skipping.");
                    continue;
                }

                await userRepository.CreateAsync(entity);
                await userRepository.SaveAsync();
            }
            catch (Exception e)
            {
                result.AddError(e);
            }

        if (result.HasError)
            logger.LogError($"One or more entities could not be seeded: {result.Errors}");

        return result;
    }
}