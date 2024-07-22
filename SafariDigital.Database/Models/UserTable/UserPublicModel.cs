using SafariDigital.Database.Models.AvatarTable;

namespace SafariDigital.Database.Models.UserTable;

public class UserPublicModel
{
    public UserPublicModel()
    {
    }

    public UserPublicModel(User user)
    {
        Id = user.Id;
        Username = user.Username;
        Email = user.Email;
        Role = user.Role;
        Avatar = user.Avatar is not null ? new AvatarModel(user.Avatar) : null;
        IsActive = user.IsActive;
        CreatedAt = user.CreatedAt;
        UpdatedAt = user.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public EUserRole Role { get; init; }
    public AvatarModel? Avatar { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}