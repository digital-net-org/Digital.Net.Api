using Microsoft.AspNetCore.Http;
using SafariDigital.Core.AppMessages;
using SafariDigital.Database.Models.User;
using SafariDigital.Services.Authentication.Models;
using SafariDigital.Services.Cache.AttemptCache;
using SafariLib.Core.HttpContext;
using SafariLib.Core.Validation;
using SafariLib.Jwt.Cache;
using SafariLib.Jwt.HttpContext;
using SafariLib.Jwt.Services;
using SafariLib.Repositories.RepositoryService;

namespace SafariDigital.Services.Authentication;

public class AuthenticationService(
    IJwtService jwtService,
    IJwtCacheService jwtCacheService,
    IAttemptCacheService attemptCache,
    IRepositoryService<User> userRepository
) : IAuthenticationService
{
    public async Task<Result<LoginResponse>> Login(
        HttpRequest request,
        HttpResponse response,
        string login,
        string password
    )
    {
        var result = new Result<LoginResponse>();

        var ipAddress = request.GetRemoteIpAddress() ?? AuthenticationUtils.DefaultIpAddress;
        var userAgent = request.GetUserAgent() ?? AuthenticationUtils.DefaultUserAgent;

        if (attemptCache.HasExceededAttempts(login, ipAddress))
            return result.AddError(EAppMessage.TooManyLoginAttempts);

        var user = await result.ValidateExpressionAsync(VerifyCredentials(login, password));

        if (result.HasError || user is null)
        {
            attemptCache.LogAttempt(login, ipAddress);
            return result;
        }

        if (!user.IsActive)
            return result.AddError(EAppMessage.UserNotActive);

        var (bearerToken, refreshToken) = GenerateTokens(user, userAgent);
        response.SetCookieToken(refreshToken);
        result.Value = new LoginResponse(bearerToken);
        attemptCache.ClearAttempts(login, ipAddress);
        return result;
    }

    public async Task<Result<LoginResponse>> RefreshTokens(
        HttpRequest request,
        HttpResponse response
    )
    {
        var result = new Result<LoginResponse>();
        var userAgent = request.GetUserAgent() ?? AuthenticationUtils.DefaultUserAgent;
        var tokenResult = jwtService.ValidateToken<AuthenticatedUser>(request.GetCookieToken());
        var userId = tokenResult.Content?.Id ?? Guid.Empty;

        result.Merge(tokenResult);

        if (result.HasError || tokenResult.Token is null)
            return result;

        var isTokenRegistered = jwtCacheService.IsTokenRegistered(
            userId,
            userAgent,
            tokenResult.Token
        );

        if (!isTokenRegistered)
            return result.AddError(EAppMessage.TokenNotKnown);

        var user = await userRepository.GetByPrimaryKeyAsync(userId);

        if (user.Value is null)
            return result.AddError(EAppMessage.TokenNotKnown);

        var (bearerToken, refreshToken) = GenerateTokens(user.Value, userAgent);
        response.SetCookieToken(refreshToken);
        result.Value = new LoginResponse(bearerToken);
        return result;
    }

    public void Logout(HttpRequest request, HttpResponse response)
    {
        var userAgent = request.GetUserAgent() ?? AuthenticationUtils.DefaultUserAgent;
        var token = request.GetCookieToken();
        var jwtToken = request.GetJwtToken<AuthenticatedUser>();

        if (token is null || jwtToken.Content is null)
            return;

        jwtCacheService.RevokeToken(jwtToken.Content.Id ?? Guid.Empty, userAgent, token);
        response.RemoveCookieToken();
    }

    public void LogoutAll(HttpRequest request, HttpResponse response)
    {
        var token = request.GetCookieToken();
        var jwtToken = request.GetJwtToken<AuthenticatedUser>();

        if (token is null || jwtToken.Content is null)
            return;

        jwtCacheService.RevokeAllTokens(jwtToken.Content.Id ?? Guid.Empty);
        response.RemoveCookieToken();
    }

    private async Task<Result<User?>> VerifyCredentials(string login, string password)
    {
        var result = await userRepository.GetFirstOrDefaultAsync(u =>
            u.Username == login || u.Email == login
        );
        return result.Value is null || !AuthenticationUtils.VerifyPassword(result.Value, password)
            ? result.AddError(EAppMessage.WrongCredentials)
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