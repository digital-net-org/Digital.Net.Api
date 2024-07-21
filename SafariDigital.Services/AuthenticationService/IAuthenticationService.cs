using SafariDigital.Core.Validation;
using SafariDigital.Services.AuthenticationService.Models;

namespace SafariDigital.Services.AuthenticationService;

public interface IAuthenticationService
{
    Task<Result<LoginResponse>> Login(string login, string password);
    Task<Result<LoginResponse>> RefreshTokens();
    void Logout();
    void LogoutAll();
}