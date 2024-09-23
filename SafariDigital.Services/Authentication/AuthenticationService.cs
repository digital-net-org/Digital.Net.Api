using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Safari.Net.Core.Extensions.HttpUtilities;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Repositories;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database;
using SafariDigital.Services.Authentication.Models;
using SafariDigital.Services.HttpContext;
using SafariDigital.Services.Jwt;

namespace SafariDigital.Services.Authentication;

public class AuthenticationService(
    IJwtService jwtService,
    IRepository<RecordedLogin> loginRecordsRepository,
    IRepository<User> userRepository,
    IHttpContextService httpContextService,
    IConfiguration configuration
) : IAuthenticationService
{
    private TimeSpan AttemptsWindow =>
        TimeSpan.FromMilliseconds(configuration.GetSection<long>(EApplicationSetting.SecurityMaxLoginWindow));

    private int MaxAttemptsAllowed =>
        configuration.GetSection<int>(EApplicationSetting.SecurityMaxLoginAttempts);

    private string CookieTokenName => configuration.GetCookieTokenName();
    private long RefreshExpiration => configuration.GetRefreshTokenExpiration();
    private HttpRequest Request => httpContextService.Request;
    private HttpResponse Response => httpContextService.Response;
    private string UserAgent => Request.GetUserAgent() ?? string.Empty;
    private string IpAddress => Request.GetRemoteIpAddress() ?? string.Empty;

    public async Task<Result<LoginResponse>> Login(string login, string password)
    {
        var result = new Result<LoginResponse>();

        if (HasReachedMaxAttempts())
            return result.AddError(EApplicationMessage.TooManyLoginAttempts);

        var user = await VerifyCredentials(login, password);
        result.Merge(user);

        if (result.HasError || user.Value is null)
        {
            await RegisterAttempt(false);
            return result;
        }

        if (!user.Value.IsActive) return result.AddError(EApplicationMessage.UserNotActive);
        var (bearerToken, refreshToken) = await GenerateTokens(user.Value);

        Response.SetCookie(refreshToken, CookieTokenName, RefreshExpiration);
        result.Value = new LoginResponse(bearerToken);
        await RegisterAttempt(true);
        return result;
    }

    public async Task<Result<LoginResponse>> RefreshTokens()
    {
        var token = Request.Cookies[CookieTokenName];
        var result = new Result<LoginResponse>();

        var tokenResult = jwtService.ValidateRefreshToken(token, UserAgent, IpAddress);
        result.Merge(tokenResult);

        var user = await userRepository.GetByIdAsync(tokenResult.Value?.Id);
        if (user is null) return result.AddError(EApplicationMessage.TokenNotKnown);

        var (bearerToken, refreshToken) = await GenerateTokens(user);
        Response.SetCookie(refreshToken, CookieTokenName, RefreshExpiration);
        result.Value = new LoginResponse(bearerToken);
        return result;
    }

    public async Task Logout()
    {
        var refreshToken = Request.Cookies[CookieTokenName];
        if (refreshToken is null) return;
        await jwtService.RevokeToken(refreshToken);
        Response.Cookies.Delete(CookieTokenName);
    }

    public string GeneratePassword(string password) => AuthenticationUtils.HashPassword(password);

    public async Task LogoutAll()
    {
        var token = Request.Cookies[CookieTokenName];
        var jwtToken = jwtService.GetJwtToken();
        if (token is null || jwtToken.Id is null) return;
        await jwtService.RevokeAllTokens(jwtToken.Id ?? Guid.Empty);
        Response.Cookies.Delete(CookieTokenName);
    }

    private async Task<Result<User>> VerifyCredentials(string login, string password)
    {
        var result = new Result<User>
        {
            Value = await userRepository
                .Get(u => u.Username == login || u.Email == login)
                .FirstOrDefaultAsync()
        };
        return result.Value is null || !AuthenticationUtils.VerifyPassword(result.Value, password)
            ? result.AddError(EApplicationMessage.WrongCredentials)
            : result;
    }

    private async Task<(string, string)> GenerateTokens(User user)
    {
        var bearerToken = jwtService.GenerateBearerToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user);
        await jwtService.RegisterToken(refreshToken, user, UserAgent, IpAddress);
        return (bearerToken, refreshToken);
    }

    private bool HasReachedMaxAttempts()
    {
        var resolvedWindow = DateTime.UtcNow.Subtract(AttemptsWindow);
        var attempts = loginRecordsRepository.Get(r => r.IpAddress == IpAddress && r.CreatedAt > resolvedWindow);
        return attempts.Count() >= MaxAttemptsAllowed && !attempts.Any(a => a.Success);
    }

    private async Task RegisterAttempt(bool success)
    {
        await loginRecordsRepository.CreateAsync(new RecordedLogin
            { IpAddress = IpAddress, UserAgent = UserAgent, Success = success });
        await loginRecordsRepository.SaveAsync();
    }
}