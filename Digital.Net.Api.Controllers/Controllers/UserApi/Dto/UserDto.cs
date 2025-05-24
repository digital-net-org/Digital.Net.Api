using Digital.Net.Api.Entities.Models.Users;

namespace Digital.Net.Api.Controllers.Controllers.UserApi.Dto;

public class UserDto
{
    public UserDto()
    {
    }

    public UserDto(User userModel)
    {
        Id = userModel.Id;
        Username = userModel.Username;
        Login = userModel.Login;
        Email = userModel.Email;
        Avatar = userModel.Avatar is not null ? new AvatarDto(userModel.Avatar) : null;
        IsActive = userModel.IsActive;
        CreatedAt = userModel.CreatedAt;
        UpdatedAt = userModel.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Login { get; init; }
    public string? Email { get; init; }
    public AvatarDto? Avatar { get; init; }
    public bool? IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}