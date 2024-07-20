using Microsoft.AspNetCore.Http;
using SafariDigital.Core.Validation;
using SafariDigital.Services.Authentication.Models;

namespace SafariDigital.Services.Authentication;

public interface IAuthenticationService
{
    Task<Result<LoginResponse>> Login(
        HttpRequest request,
        HttpResponse response,
        string login,
        string password
    );

    Task<Result<LoginResponse>> RefreshTokens(HttpRequest request, HttpResponse response);
    void Logout(HttpRequest request, HttpResponse response);
    void LogoutAll(HttpRequest request, HttpResponse response);
}