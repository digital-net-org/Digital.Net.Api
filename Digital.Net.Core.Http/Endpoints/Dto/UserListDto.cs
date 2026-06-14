using Digital.Net.Core.Entities.Models.Users;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class UserListDto
{
    public UserListDto()
    {
    }

    public UserListDto(User userModel)
    {
        Id = userModel.Id;
        Username = userModel.Username;
        Login = userModel.Login;
        Email = userModel.Email;
        IsActive = userModel.IsActive;
        IsAdmin = userModel.IsAdmin;
        CreatedAt = userModel.CreatedAt;
        UpdatedAt = userModel.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Login { get; init; }
    public string? Email { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsAdmin { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}