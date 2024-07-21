using SafariDigital.Database.Models.User;

namespace SafariDigital.Services.Jwt.Models;

public class AuthenticatedUser
{
    public AuthenticatedUser()
    {
    }

    public AuthenticatedUser(Guid? id, EUserRole? role)
    {
        Id = id;
        Role = role;
    }

    public AuthenticatedUser(User user)
    {
        Id = user.Id;
        Role = user.Role;
    }

    public Guid? Id { get; init; }
    public EUserRole? Role { get; init; }
}