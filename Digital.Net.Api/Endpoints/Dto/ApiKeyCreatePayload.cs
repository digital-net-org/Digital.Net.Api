namespace Digital.Net.Api.Endpoints.Dto;

public class ApiKeyCreatePayload
{
    public required string Name { get; set; }
    public DateTime? ExpiredAt { get; set; }
}
