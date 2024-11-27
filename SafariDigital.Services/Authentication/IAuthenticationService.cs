using Digital.Net.Core.Messages;
using SafariDigital.Services.Authentication.Models;

namespace SafariDigital.Services.Authentication;

public interface IAuthenticationService
{
    string GeneratePassword(string password);
    Task<Result<LoginResponse>> Login(string login, string password);
    Task<Result<LoginResponse>> RefreshTokens();
    Task Logout();
    Task LogoutAll();
}