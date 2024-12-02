using Digital.Net.Entities.Repositories;
using Digital.Net.Mvc.Services;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Services.Authentication.Service;

public class AuthenticatedUserService(
    IHttpContextService contextAccessor,
    IRepository<User> userRepository
) : IAuthenticatedUserService
{
    public const string Token = "Token";

    public async Task<User> GetAuthenticatedUser()
    {
        var token = contextAccessor.GetItem<AuthenticatedUser>(Token);
        var user = await userRepository.GetByIdAsync(token?.Id);
        return user ?? throw new NullReferenceException("No user authenticated");
    }
}