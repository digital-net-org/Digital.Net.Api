using Digital.Net.Core.Entities.Models.Auth;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class AuthEventDto
{
    public AuthEventDto() { }

    public AuthEventDto(AuthEvent authEvent)
    {
        Id = authEvent.Id;
        Type = authEvent.Type;
        Success = authEvent.Success;
        Login = authEvent.Login;
        UserId = authEvent.UserId;
        IpAddress = authEvent.IpAddress;
        UserAgent = authEvent.UserAgent;
        CreatedAt = authEvent.CreatedAt;
    }

    public Guid Id { get; init; }
    public AuthEventType Type { get; init; }
    public bool Success { get; init; }
    public string? Login { get; init; }
    public Guid? UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime CreatedAt { get; init; }
}
