using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Core.Services.ApiKeys;

public interface IApiKeyService
{
    Task<Result<string>> CreateAsync(Guid userId, string name, DateTime? expiresAt);
    Task<Result<List<ApiKey>>> GetByUserAsync(Guid userId);
    Task<Result> DeleteAsync(Guid userId, Guid keyId);
}
