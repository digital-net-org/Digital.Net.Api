using System.Diagnostics;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Services.Authentication.Exceptions;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Services.Users;
using Digital.Net.Core.Services.Users.Exceptions;
using Digital.Net.Lib.Accessors;
using Digital.Net.Lib.Environment;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Http.Services.Authentication;

public class AuthenticationService(
    SessionService sessionService,
    AuthEventService authEventService,
    IOriginAccessor originAccessor,
    IUserAccessor userAccessor,
    DigitalContext context
)
{
    public async Task<Result<string>> LoginAsync(LoginPayload payload)
    {
        var startedAt = Stopwatch.GetTimestamp();

        var result = new Result<string>(string.Empty);
        var origin = originAccessor.GetOrigin();
        if (string.IsNullOrWhiteSpace(origin.IpAddress))
            return result.AddError(new IpNotFound());
        if (payload.Login.Length is > 48 or < 1 || payload.Password.Length is > 256 or < 1)
            return result.AddError(new InvalidLoginPayloadException());

        var user = await context.Users.FirstOrDefaultAsync(u => u.Login == payload.Login);
        if (await authEventService.HasReachedMaxLoginAttemptsAsync(origin.IpAddress, user?.Id))
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

        result.Value = await sessionService.CreateAsync(user.Id, origin.UserAgent ?? string.Empty);
        return result;
    }

    public async Task<Result> LogoutAsync(string sessionId)
    {
        var result = new Result();
        var origin = originAccessor.GetOrigin();
        var userId = userAccessor.GetUserId();
        await sessionService.RevokeAsync(sessionId);
        await authEventService.RecordAsync(AuthEventType.Logout, true, origin.IpAddress, origin.UserAgent, userId);
        return result;
    }

    public async Task<Result> LogoutAllAsync()
    {
        var result = new Result();
        var origin = originAccessor.GetOrigin();
        var userId = userAccessor.GetUserId();
        await sessionService.RevokeAllAsync(userId);
        await authEventService.RecordAsync(AuthEventType.LogoutAll, true, origin.IpAddress, origin.UserAgent, userId);
        return result;
    }
}
