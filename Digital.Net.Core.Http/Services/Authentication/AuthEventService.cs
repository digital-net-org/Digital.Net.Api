using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Http.Services.Authentication;

public class AuthEventService(DigitalContext context, AuthenticationOptionService options)
{
    public async Task RecordAsync(
        AuthEventType type,
        bool success,
        string? ipAddress,
        string? userAgent,
        Guid? userId,
        string? login = null
    )
    {
        await context.AuthEvents.AddAsync(new AuthEvent
        {
            Type = type,
            Success = success,
            Login = login,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });
        await context.SaveChangesAsync();
    }

    public async Task<bool> HasReachedMaxLoginAttemptsAsync(string ipAddress)
    {
        var threshold = DateTime.UtcNow.Subtract(options.GetMaxLoginAttemptsThreshold());
        return await context.AuthEvents.CountAsync(e =>
            e.CreatedAt > threshold
            && e.Type == AuthEventType.Login
            && !e.Success
            && e.IpAddress == ipAddress
        ) >= AuthenticationStaticOptions.MaxLoginAttempts;
    }
}