using Digital.Net.Api.Endpoints.Dto;

namespace Digital.Net.Api.Services.Users.Events.Types;

public class AdminUserMutationEvent
{
    public AdminUserMutationEvent()
    {
    }

    public AdminUserMutationEvent(UserPayload payload, Guid userId)
    {
        Username = payload.Username;
        Login = payload.Login;
        Email = payload.Email;
        Id = userId;
    }

    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}