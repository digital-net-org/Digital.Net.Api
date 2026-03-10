using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.ApiKeys;

namespace Digital.Net.Api.Services.ApiKeys;

public interface IApiKeyService
{
    Task<Result<string>> CreateAsync(Guid userId, string name, DateTime? expiresAt);
    Task<Result<List<ApiKey>>> GetByUserAsync(Guid userId);
    Task<Result> DeleteAsync(Guid userId, Guid keyId);
}
