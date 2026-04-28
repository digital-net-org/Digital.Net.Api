using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Services.ApiKeys.Exceptions;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Users.Events;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Random;
using Digital.Net.Lib.String;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Services.ApiKeys;

public class ApiKeyService(
    DigitalContext context,
    IAuditService auditService
)
{
    public const int MaxApiKeysPerUser = 5;
    public static readonly TimeSpan DefaultExpiration = TimeSpan.FromDays(90);

    public async Task<Result<string>> CreateAsync(Guid userId, string name, DateTime? expiresAt)
    {
        var result = new Result<string>();
        try
        {
            if (!RegularExpressions.ApiKeyName.IsMatch(name))
                throw new ApiKeyNameMalformedException();

            if (expiresAt.HasValue && expiresAt.Value < DateTime.UtcNow)
                throw new ExpiredAtInThePastException();

            var userKeyCount = await context.ApiKeys.CountAsync(k => k.UserId == userId);
            if (userKeyCount >= MaxApiKeysPerUser)
                throw new MaxApiKeysReachedException();

            var nameExists = await context.ApiKeys.AnyAsync(k => k.UserId == userId && k.Name == name);
            if (nameExists)
                throw new DuplicateApiKeyNameException();

            var plaintextKey = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 128);
            var apiKey = new ApiKey(userId, name, plaintextKey, expiresAt ?? DateTime.UtcNow.Add(DefaultExpiration));

            await context.ApiKeys.AddAsync(apiKey);
            await context.SaveChangesAsync();
            result.Value = plaintextKey;

            await auditService.RegisterEventAsync(
                UserEvents.CreateApiKey,
                EventState.Success,
                result,
                userId
            );
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        
        return result;
    }

    public async Task<Result<List<ApiKey>>> GetByUserAsync(Guid userId)
    {
        var result = new Result<List<ApiKey>>
        {
            Value = await context.ApiKeys
                .Where(k => k.UserId == userId)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync()
        };
        return result;
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid keyId)
    {
        var result = new Result();
        try
        {
            var apiKey = await context.ApiKeys.FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);
            if (apiKey is null) throw new KeyNotFoundException("API key not found.");
            context.ApiKeys.Remove(apiKey);
            await context.SaveChangesAsync();
            await auditService.RegisterEventAsync(
                UserEvents.DeleteApiKey,
                EventState.Success,
                result,
                userId
            );
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        return result;
    }
}
