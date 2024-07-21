using Microsoft.Extensions.Configuration;
using SafariDigital.Core.Application;
using SafariDigital.Core.Validation;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Database.Repository;
using SafariDigital.Services.AuthenticationService.Models;
using SafariDigital.Services.CacheService.AttemptCache;
using SafariDigital.Services.CacheService.JwtCache;
using SafariDigital.Services.HttpContextService;
using SafariDigital.Services.JwtService;

namespace SafariDigital.Services.AuthenticationService;

public class AuthenticationService(
    IJwtService jwtService,
    IJwtCacheService jwtCacheService,
    IAttemptCacheService attemptCache,
    IRepositoryService<User> userRepository,
    IHttpContextService httpContextService,
    IConfiguration configuration
) : IAuthenticationService
{
    private readonly string _cookieTokenName = configuration.GetCookieTokenName();
    private readonly long _refreshExpiration = configuration.GetRefreshTokenExpiration();

    public void LogoutAll()
    {
        var (request, response) = httpContextService.GetControllerContext();
        var token = request.Cookies[_cookieTokenName];
        var jwtToken = jwtService.GetJwtToken();
        if (token is null || jwtToken.Content is null) return;

        jwtCacheService.RevokeAllTokens(jwtToken.Content.Id ?? Guid.Empty);
        response.Cookies.Delete(_cookieTokenName);
    }

    public async Task<Result<LoginResponse>> Login(
        string login,
        string password
    )
    {
        var (request, response) = httpContextService.GetControllerContext();
        var ipAddress = request.GetRemoteIpAddress();
        var userAgent = request.GetUserAgent();
        var result = new Result<LoginResponse>();

        if (attemptCache.HasExceededAttempts(login, ipAddress))
            return result.AddError(EApplicationMessage.TooManyLoginAttempts);

        var user = await result.ValidateExpressionAsync(VerifyCredentials(login, password));

        if (result.HasError || user is null)
        {
            attemptCache.LogAttempt(login, ipAddress);
            return result;
        }

        if (!user.IsActive) return result.AddError(EApplicationMessage.UserNotActive);
        var (bearerToken, refreshToken) = GenerateTokens(user, userAgent);

        response.SetCookie(refreshToken, _cookieTokenName, _refreshExpiration);
        result.Value = new LoginResponse(bearerToken);
        attemptCache.ClearAttempts(login, ipAddress);
        return result;
    }

    public async Task<Result<LoginResponse>> RefreshTokens()
    {
        var (request, response) = httpContextService.GetControllerContext();
        var userAgent = request.GetUserAgent();
        var token = request.Cookies[_cookieTokenName];
        var result = new Result<LoginResponse>();

        var tokenResult = jwtService.ValidateToken(token);
        result.Merge(tokenResult);
        if (result.HasError || tokenResult.Token is null) return result;

        var userId = tokenResult.Content?.Id ?? Guid.Empty;
        var isTokenRegistered = jwtCacheService.IsTokenRegistered(
            userId,
            userAgent,
            tokenResult.Token
        );
        if (!isTokenRegistered) return result.AddError(EApplicationMessage.TokenNotKnown);

        var user = await userRepository.GetByPrimaryKeyAsync(userId);
        if (user.Value is null) return result.AddError(EApplicationMessage.TokenNotKnown);

        var (bearerToken, refreshToken) = GenerateTokens(user.Value, userAgent);
        response.SetCookie(refreshToken, _cookieTokenName, _refreshExpiration);
        result.Value = new LoginResponse(bearerToken);
        return result;
    }

    public void Logout()
    {
        var (request, response) = httpContextService.GetControllerContext();
        var userAgent = request.GetUserAgent();
        var refreshToken = request.Cookies[_cookieTokenName];
        var jwtToken = jwtService.GetJwtToken();

        if (refreshToken is null || jwtToken.Content is null) return;

        jwtCacheService.RevokeToken(jwtToken.Content.Id ?? Guid.Empty, userAgent, refreshToken);
        response.Cookies.Delete(_cookieTokenName);
    }

    private async Task<Result<User?>> VerifyCredentials(string login, string password)
    {
        var result = await userRepository.GetFirstOrDefaultAsync(u =>
            u.Username == login || u.Email == login
        );
        return result.Value is null || !AuthenticationUtils.VerifyPassword(result.Value, password)
            ? result.AddError(EApplicationMessage.WrongCredentials)
            : result;
    }

    private (string, string) GenerateTokens(User user, string userAgent)
    {
        var bearerToken = jwtService.GenerateBearerToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user);
        jwtCacheService.RegisterToken(user.Id, userAgent, refreshToken);
        return (bearerToken, refreshToken);
    }
}