using System.Security.Authentication;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Authentication.Events;
using Digital.Net.Api.Services.Authentication.Exceptions;
using Digital.Net.Api.Services.Authentication.Options;
using Digital.Net.Api.Services.Authentication.Services.Authorization;
using Digital.Net.Api.Services.Events;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Services.Authentication.Services.Authentication;

public class AuthenticationService(
    IAuthenticationOptionService authenticationOptionService,
    IAuthenticationJwtService authenticationJwtService,
    IAuthorizationJwtService authorizationJwtService,
    IEventService eventService,
    IRepository<User, DigitalContext> userRepository,
    IRepository<ApiToken, DigitalContext> tokenRepository,
    IRepository<Event, DigitalContext> eventRepository
) : IAuthenticationService
{
    private async Task<int> GetLoginAttemptCountAsync(User? user = null, string? ipAddress = null)
    {
        if (user is null)
            return 0;
        ipAddress ??= string.Empty;
        var threshold = DateTime.UtcNow.Subtract(authenticationOptionService.GetMaxLoginAttemptsThreshold());
        return await eventRepository.CountAsync(
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
            Value = await userRepository.Get(u => u.Login == login).FirstOrDefaultAsync()
        };

        if (await GetLoginAttemptCountAsync(result.Value) >= DefaultAuthenticationOptions.MaxLoginAttempts)
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

        await eventService.RegisterEventAsync(
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
            authenticationJwtService.GenerateBearerToken(userResult.Value!.Id, userAgent),
            authenticationJwtService.GenerateRefreshToken(userResult.Value.Id, userAgent)
        );
        return result;
    }

    public async Task<Result<(string bearer, string? refresh)>> RefreshTokensAsync(string? refreshToken, string? userAgent = null)
    {
        var result = new Result<(string, string?)>((string.Empty, null));
        var tokenResult = authorizationJwtService.AuthorizeRefreshToken(refreshToken);
        
        result.Merge(tokenResult);
        if (result.HasError)
            return result;
        if (tokenResult.ShouldRenewCookie)
            await authenticationJwtService.RevokeTokenAsync(refreshToken!);
        
        result.Value = (
            authenticationJwtService.GenerateBearerToken(tokenResult.UserId, userAgent ?? string.Empty),
            tokenResult.ShouldRenewCookie 
                ? authenticationJwtService.GenerateRefreshToken(tokenResult.UserId, userAgent ?? string.Empty)
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

        await authenticationJwtService.RevokeTokenAsync(refreshToken);
        await eventService.RegisterEventAsync(
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
        var userId = tokenRepository.Get(u => u.Key == refreshToken).FirstOrDefault()?.UserId;
        if (userId is null)
            return result.AddError(new AuthenticationException());

        await authenticationJwtService.RevokeAllTokensAsync(userId.Value);
        await eventService.RegisterEventAsync(
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