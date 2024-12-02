using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Services.Authentication.Service;

public interface IAuthenticatedUserService
{
    Task<User> GetAuthenticatedUser();
}