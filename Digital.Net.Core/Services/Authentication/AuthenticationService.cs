using System.Security.Authentication;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication.Events;
using Digital.Net.Core.Services.Authentication.Exceptions;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Core.Services.Authentication.Utils;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Services.Authentication;

public class AuthenticationService(
    IAuthenticationOptionService authenticationOptionService,
    IJwtService jwtService,
    IAuditService auditService,
    DigitalContext context
) : IAuthenticationService
{
    private async Task<int> GetLoginAttemptCountAsync(User? user = null, string? ipAddress = null)
    {
        if (user is null)
            return 0;
        ipAddress ??= string.Empty;
        var threshold = DateTime.UtcNow.Subtract(authenticationOptionService.GetMaxLoginAttemptsThreshold());
        return await context.Events.CountAsync(
            e =>
                e.CreatedAt > threshold
                && e.Name == AuthenticationEvents.Login
                && e.State == EventState.Failed
                && e.IpAddress == ipAddress
                && e.UserId == user.Id
        );
    }

    public async Task<Result<User>> ValidateCredentialsAsync(string login, string password)
    {
        var result = new Result<User>
        {
            Value = await context.Users.FirstOrDefaultAsync(u => u.Login == login)
        };

        if (await GetLoginAttemptCountAsync(result.Value) >= AuthenticationStaticOptions.MaxLoginAttempts)
            result.AddError(new TooManyAttemptsException());
        else if (result.Value is null)
            result.AddError(new InvalidCredentialsException());
        else if (!result.Value.IsActive)
            result.AddError(new InactiveUserException());
        else if (!PasswordUtils.VerifyPassword(result.Value, password))
            result.AddError(new InvalidCredentialsException());
        return result;
    }

    public async Task<Result<(string bearer, string refresh)>> LoginAsync(
        string login,
        string password,
        string? userAgent = null,
        string? ipAddress = null
    )
    {
        var result = new Result<(string, string)>((string.Empty, string.Empty));
        userAgent ??= string.Empty;
        var userResult = await ValidateCredentialsAsync(login, password);
        var state = userResult.HasError ? EventState.Failed : EventState.Success;

        result.Merge(userResult);

        await auditService.RegisterEventAsync(
            AuthenticationEvents.Login,
            state,
            result,
            userResult.Value?.Id,
            login,
            userAgent,
            ipAddress
        );

        if (result.HasError)
            return result;

        result.Value = (
            jwtService.GenerateBearerToken(userResult.Value!.Id, userAgent),
            jwtService.GenerateRefreshToken(userResult.Value.Id, userAgent)
        );
        return result;
    }

    public async Task<Result<(string bearer, string? refresh)>> RefreshTokensAsync(string? refreshToken, string? userAgent = null)
    {
        var result = new Result<(string, string?)>((string.Empty, null));
        var apiToken = context.ApiTokens.FirstOrDefault(a => a.Key == (refreshToken ?? string.Empty));
        if (apiToken is null)
            return result.AddError(new InvalidTokenException());

        var tokenResult = jwtService.AuthorizeToken(apiToken.Key);
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
        var userId = context.ApiTokens.FirstOrDefault(u => u.Key == refreshToken)?.UserId;
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