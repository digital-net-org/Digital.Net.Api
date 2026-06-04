using Digital.Net.Core.Entities.Models.ApiKeys;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class ApiKeyDto
{
    public ApiKeyDto()
    {
    }

    public ApiKeyDto(ApiKey apiKey)
    {
        Id = apiKey.Id;
        Name = apiKey.Name;
        CreatedAt = apiKey.CreatedAt;
        ExpiredAt = apiKey.ExpiredAt;
    }

    public Guid Id { get; init; }
    public string? Name { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiredAt { get; init; }
}
