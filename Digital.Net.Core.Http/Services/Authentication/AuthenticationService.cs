using System.Diagnostics;
using System.Security.Authentication;
using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiTokens;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Services.Authentication.Exceptions;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Services.Users;
using Digital.Net.Core.Services.Users.Exceptions;
using Digital.Net.Lib.Environment;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Http.Services.Authentication;

public class AuthenticationService(
    JwtService jwtService,
    AuthEventService authEventService,
    IOriginAccessor originAccessor,
    IUserAccessor userAccessor,
    DigitalContext context
)
{
    public async Task<Result<(string bearer, string refresh)>> LoginAsync(LoginPayload payload)
    {
        var startedAt = Stopwatch.GetTimestamp();
        
        var result = new Result<(string, string)>((string.Empty, string.Empty));
        var origin = originAccessor.GetOrigin();
        if (string.IsNullOrWhiteSpace(origin.IpAddress))
            return result.AddError(new IpNotFound());
        if (payload.Login.Length is > 48 or < 1 || payload.Password.Length is > 256 or < 1)
            return result.AddError(new InvalidLoginPayloadException());

        var user = await context.Users.FirstOrDefaultAsync(u => u.Login == payload.Login);
        if (await authEventService.HasReachedMaxLoginAttemptsAsync(origin.IpAddress))
            result.AddError(new TooManyAttemptsException());
        else if (user is null)
            result.AddError(new InvalidCredentialsException());
        else if (!user.IsActive)
            result.AddError(new InactiveUserException());
        else if (!UserPassword.Verify(user, payload.Password))
            result.AddError(new InvalidCredentialsException());

        await authEventService.RecordAsync(
            AuthEventType.Login,
            !result.HasError,
            origin.IpAddress,
            origin.UserAgent, 
            user?.Id,
            payload.Login
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
            jwtService.GenerateBearerToken(user.Id, origin.UserAgent ?? string.Empty),
            jwtService.GenerateRefreshToken(user.Id, origin.UserAgent ?? string.Empty)
        );
        return result;
    }

    public async Task<Result<(string bearer, string? refresh)>> RefreshTokensAsync(string? refreshToken,
        string? userAgent = null)
    {
        var result = new Result<(string, string?)>((string.Empty, null));
        var hashedToken = ApiToken.Hash(refreshToken ?? string.Empty);
        var apiToken = context.ApiTokens.FirstOrDefault(a => a.Key == hashedToken);
        if (apiToken is null)
            return result.AddError(new InvalidTokenException());
        if (apiToken.UserAgent != (userAgent ?? string.Empty))
            return result.AddError(new InvalidTokenException());

        var tokenResult = await jwtService.AuthorizeTokenAsync(refreshToken);
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

    public async Task<Result> LogoutAsync(string refreshToken)
    {
        var result = new Result();
        var origin = originAccessor.GetOrigin();
        var userId = userAccessor.GetUserId();
        await jwtService.RevokeTokenAsync(refreshToken);
        await authEventService.RecordAsync(AuthEventType.Logout, true, origin.IpAddress, origin.UserAgent, userId);
        return result;
    }

    public async Task<Result> LogoutAllAsync(string refreshToken)
    {
        var result = new Result();
        var origin = originAccessor.GetOrigin();
        var userId = context.ApiTokens.FirstOrDefault(u => u.Key == ApiToken.Hash(refreshToken))?.UserId;
        if (userId is null)
            return result.AddError(new AuthenticationException());

        await jwtService.RevokeAllTokensAsync(userId.Value);
        await authEventService.RecordAsync(AuthEventType.LogoutAll, true, origin.IpAddress, origin.UserAgent, userId);
        return result;
    }
}
