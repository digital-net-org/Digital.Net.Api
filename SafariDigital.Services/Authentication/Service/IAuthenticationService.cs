using Digital.Net.Core.Messages;
using SafariDigital.Services.Authentication.Models;

namespace SafariDigital.Services.Authentication.Service;

public interface IAuthenticationService
{
    string GeneratePassword(string password);
    Task<Result<TokenResult>> Login(string login, string password);
    Task<Result<TokenResult>> RefreshTokens();
    Task Logout();
    Task LogoutAll();
}