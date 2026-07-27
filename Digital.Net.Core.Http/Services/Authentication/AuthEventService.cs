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

    /// <summary>
    ///     Whether logins must be refused, either because this IP failed too often or because this
    ///     account did.
    /// </summary>
    public async Task<bool> HasReachedMaxLoginAttemptsAsync(string ipAddress, Guid? userId = null)
    {
        var threshold = DateTime.UtcNow.Subtract(options.GetMaxLoginAttemptsThreshold());
        var failedLogins = context.AuthEvents.Where(e =>
            e.CreatedAt > threshold && e.Type == AuthEventType.Login && !e.Success
        );

        var perIpFailureCounts = await failedLogins.CountAsync(e => e.IpAddress == ipAddress);
        if (perIpFailureCounts >= AuthenticationStaticOptions.MaxLoginAttempts)
            return true;

        if (userId is null)
            return false;

        var perUserFailureCounts = await failedLogins.CountAsync(e => e.UserId == userId);
        return perUserFailureCounts >= AuthenticationStaticOptions.MaxAccountLoginAttempts;
    }
}