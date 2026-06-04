namespace Digital.Net.Core.Services.Users.Events.Types;

public class AdminUserMutationEvent
{
    public AdminUserMutationEvent()
    {
    }

    public AdminUserMutationEvent(string username, string login, string email, Guid userId)
    {
        Username = username;
        Login = login;
        Email = email;
        Id = userId;
    }

    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}