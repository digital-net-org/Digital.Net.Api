using SafariDigital.Database.Models.User;

namespace SafariDigital.Services.Authentication.Models;

public class AuthenticatedUser(Guid? id, EUserRole? role)
{
    public Guid? Id { get; } = id;
    public EUserRole? Role { get; } = role;
}