using Digital.Pages.Data.Models.Users;

namespace Digital.Pages.Api.Dto.Entities;

public class UserModel
{
    public UserModel()
    {
    }

    public UserModel(User userModel)
    {
        Id = userModel.Id;
        Username = userModel.Username;
        Login = userModel.Login;
        Email = userModel.Email;
        Role = userModel.Role;
        Avatar = userModel.Avatar is not null ? new AvatarModel(userModel.Avatar) : null;
        IsActive = userModel.IsActive;
        CreatedAt = userModel.CreatedAt;
        UpdatedAt = userModel.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Login { get; init; }
    public string? Email { get; init; }
    public UserRole Role { get; init; }
    public AvatarModel? Avatar { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}