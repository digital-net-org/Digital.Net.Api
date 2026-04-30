using System.Diagnostics;
using System.Security.Authentication;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiTokens;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication.Events;
using Digital.Net.Core.Services.Authentication.Exceptions;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Core.Services.Authentication.Utils;
using Digital.Net.Lib.Environment;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Services.Authentication;

public class AuthenticationService(
    AuthenticationOptionService authenticationOptionService,
    JwtService jwtService,
    IAuditService auditService,
    DigitalContext context
)
{
    public async Task<int> GetLoginAttemptCountAsync(string ipAddress)
    {
        var threshold = DateTime.UtcNow.Subtract(authenticationOptionService.GetMaxLoginAttemptsThreshold());
        return await context.Events.CountAsync(
            e =>
                e.CreatedAt > threshold
                && e.Name == AuthenticationEvents.Login
                && e.State == EventState.Failed
                && e.IpAddress == ipAddress
        );
    }
    
    public async Task<Result<(string bearer, string refresh)>> LoginAsync(
        string login,
        string password,
        string ipAddress,
        string? userAgent = null
    )
    {
        var startedAt = Stopwatch.GetTimestamp();
        var result = new Result<(string, string)>((string.Empty, string.Empty));
        userAgent ??= string.Empty;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Login == login);
        if (await GetLoginAttemptCountAsync(ipAddress) >= AuthenticationStaticOptions.MaxLoginAttempts)
            result.AddError(new TooManyAttemptsException());
        else if (user is null)
            result.AddError(new InvalidCredentialsException());
        else if (!user.IsActive)
            result.AddError(new InactiveUserException());
        else if (!PasswordUtils.VerifyPassword(user, password))
            result.AddError(new InvalidCredentialsException());

        var state = result.HasError ? EventState.Failed : EventState.Success;
        await auditService.RegisterEventAsync(
            AuthenticationEvents.Login,
            state,
            result,
            user?.Id,
            login,
            userAgent,
            ipAddress
        );

        if (!AspNetEnv.IsTest)
        {
            var elapsed = Stopwatch.GetElapsedTime(startedAt);
            var remaining = TimeSpan.FromMilliseconds(AuthenticationStaticOptions.MinLoginDurationMs) - elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining);
        }

        if (result.HasError || user is null)
            return result;

        result.Value = (
            jwtService.GenerateBearerToken(user.Id, userAgent),
            jwtService.GenerateRefreshToken(user.Id, userAgent)
        );
        return result;
    }

    public async Task<Result<(string bearer, string? refresh)>> RefreshTokensAsync(string? refreshToken, string? userAgent = null)
    {
        var result = new Result<(string, string?)>((string.Empty, null));
        var hashedToken = ApiToken.Hash(refreshToken ?? string.Empty);
        var apiToken = context.ApiTokens.FirstOrDefault(a => a.Key == hashedToken);
        if (apiToken is null)
            return result.AddError(new InvalidTokenException());
        if (apiToken.UserAgent != (userAgent ?? string.Empty))
            return result.AddError(new InvalidTokenException());

        var tokenResult = jwtService.AuthorizeToken(refreshToken);
        result.Merge(tokenResult);
        
        if (result.HasError)
            return result;
        if (tokenResult.ShouldRenewCookie)
            await jwtService.RevokeTokenAsync(refreshToken!);
        
        result.Value = (
            jwtService.GenerateBearerToken(tokenResult.UserId, userAgent ?? string.Empty),
            tokenResult.ShouldRenewCookie
                ? jwtService.GenerateRefreshToken(tokenResult.UserId, userAgent ?? string.Empty)
                : null
        );
        return result;
    }

    public async Task<Result> LogoutAsync(
        string? refreshToken,
        Guid? userId,
        string? userAgent = null,
        string? ipAddress = null
    )
    {
        var result = new Result();
        if (refreshToken is null)
            return result.AddError(new AuthenticationException());

        await jwtService.RevokeTokenAsync(refreshToken);
        await auditService.RegisterEventAsync(
            AuthenticationEvents.Logout,
            EventState.Success,
            null,
            userId,
            null,
            userAgent,
            ipAddress
        );
        return result;
    }

    public async Task<Result> LogoutAllAsync(
        string? refreshToken,
        string? userAgent = null,
        string? ipAddress = null
    )
    {
        var result = new Result();
        var userId = context.ApiTokens.FirstOrDefault(u => u.Key == ApiToken.Hash(refreshToken ?? string.Empty))?.UserId;
        if (userId is null)
            return result.AddError(new AuthenticationException());

        await jwtService.RevokeAllTokensAsync(userId.Value);
        await auditService.RegisterEventAsync(
            AuthenticationEvents.LogoutAll,
            EventState.Success,
            null,
            userId,
            null,
            userAgent,
            ipAddress
        );
        return result;
    }
}