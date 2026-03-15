namespace Digital.Net.Core.Endpoints.Dto;

public class UserRolePayload
{
    public required bool IsAdmin { get; init; }
    public required string Password { get; init; }
}
