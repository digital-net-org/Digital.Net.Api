using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Sessions;
using Digital.Net.Core.Http.Services.Authentication.Exceptions;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Http.Services.Authentication.Types;
using Digital.Net.Lib.Random;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Http.Services.Authentication;

public class SessionService(
    AuthenticationOptionService options,
    DigitalContext context
)
{
    /// <summary>Creates a session and returns its opaque id in clear text — the only time it ever exists.</summary>
    public async Task<string> CreateAsync(Guid userId, string userAgent, CancellationToken ct = default)
    {
        var sessionId = Randomizer.GenerateRandomString(
            Randomizer.AnyLetterOrNumber,
            AuthenticationStaticOptions.SessionIdLength
        );
        var now = DateTime.UtcNow;

        await EvictSurplusSessionsAsync(userId, now, ct);
        context.Sessions.Add(
            new Session(
                userId,
                Session.Hash(sessionId),
                userAgent,
                options.GetIdleExpirationDate(now),
                options.GetAbsoluteExpirationDate(now)
            ));
        await context.SaveChangesAsync(ct);

        return sessionId;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(string? sessionId, CancellationToken ct = default)
    {
        var result = new AuthorizationResult();
        if (string.IsNullOrWhiteSpace(sessionId))
            return result.AddError(new TokenNotFoundException());

        var hash = Session.Hash(sessionId);
        var session = await context.Sessions
            .Where(s => s.Key == hash)
            .Select(s => new { s.Id, s.UserId, s.ExpiredAt, s.AbsoluteExpiredAt, s.User.IsActive })
            .FirstOrDefaultAsync(ct);

        if (session is null)
            return result.AddError(new InvalidTokenException());

        var now = DateTime.UtcNow;
        if (now >= session.ExpiredAt || now >= session.AbsoluteExpiredAt)
        {
            await context.Sessions.Where(s => s.Id == session.Id).ExecuteDeleteAsync(ct);
            return result.AddError(new ExpiredTokenException());
        }

        if (!session.IsActive)
            return result.AddError(new InvalidTokenException());

        await SlideAsync(session.Id, session.ExpiredAt, session.AbsoluteExpiredAt, now, ct);

        result.Authorize(session.UserId);
        result.Scheme = AuthorizeType.Session;
        return result;
    }

    public async Task RevokeAsync(string sessionId, CancellationToken ct = default)
    {
        var hash = Session.Hash(sessionId);
        await context.Sessions.Where(s => s.Key == hash).ExecuteDeleteAsync(ct);
    }

    public async Task RevokeAllAsync(Guid userId, CancellationToken ct = default) =>
        await context.Sessions.Where(s => s.UserId == userId).ExecuteDeleteAsync(ct);

    private async Task SlideAsync(
        Guid sessionId,
        DateTime currentExpiredAt,
        DateTime absoluteExpiredAt,
        DateTime now,
        CancellationToken ct
    )
    {
        var target = now.Add(options.IdleWindow);
        if (target > absoluteExpiredAt)
            target = absoluteExpiredAt;

        // ExpiredAt is always lastSlide + IdleWindow, so (target - ExpiredAt) is the time since the last
        // slide: throttling on it keeps the hot path to one write per session per threshold.
        if (target - currentExpiredAt < options.RenewalThreshold)
            return;

        // ExecuteUpdateAsync bypasses TimestampInterceptor, so UpdatedAt has to be set by hand — the
        // eviction below orders on it.
        await context.Sessions
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(x => x.ExpiredAt, target)
                    .SetProperty(x => x.UpdatedAt, now),
                ct);
    }

    private async Task EvictSurplusSessionsAsync(Guid userId, DateTime now, CancellationToken ct)
    {
        var maxAllowed = AuthenticationStaticOptions.MaxConcurrentSessions;
        var sessions = await context.Sessions
            .Where(s => s.UserId == userId && s.ExpiredAt > now)
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .ToListAsync(ct);

        if (sessions.Count < maxAllowed)
            return;

        context.Sessions.RemoveRange(sessions.Skip(maxAllowed - 1));
    }
}