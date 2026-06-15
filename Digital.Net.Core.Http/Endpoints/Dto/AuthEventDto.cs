using Digital.Net.Core.Entities.Models.Auth;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class AuthEventDto
{
    public AuthEventDto() { }

    public Guid Id { get; init; }
    public AuthEventType Type { get; init; }
    public bool Success { get; init; }
    public string? Login { get; init; }
    public Guid? UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime CreatedAt { get; init; }
}
