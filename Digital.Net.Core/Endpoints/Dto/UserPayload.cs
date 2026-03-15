namespace Digital.Net.Core.Endpoints.Dto;

public class UserPayload
{
    public required string Username { get; init; }
    public required string Login { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public bool? IsActive { get; init; } = false;
}