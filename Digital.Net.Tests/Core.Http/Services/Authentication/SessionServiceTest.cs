using Digital.Net.Core;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Sessions;
using Digital.Net.Core.Http.Services.Authentication;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Digital.Net.Tests.Core.Http.Services.Authentication;

public class SessionServiceTest : DbServiceTest<DigitalContext>
{
    private const long IdleMs = 7200000; // 2 h
    private const long AbsoluteMs = 604800000; // 7 j

    private SessionService _service = null!;

    protected override Task OnInitializedAsync()
    {
        _service = BuildService();
        return Task.CompletedTask;
    }

    private SessionService BuildService(long idleMs = IdleMs, long absoluteMs = AbsoluteMs)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [CoreSettings.SessionIdleExpirationKey] = idleMs.ToString(),
                    [CoreSettings.SessionAbsoluteExpirationKey] = absoluteMs.ToString()
                })
            .Build();
        var options = Options.Create(new AuthenticationOptions { CookieSameSite = SameSiteMode.Lax });
        return new SessionService(new AuthenticationOptionService(configuration, options), Context);
    }

    private async Task<Session> GetStoredAsync(string sessionId)
    {
        var hash = Session.Hash(sessionId);
        return await Context.Sessions.AsNoTracking().FirstAsync(s => s.Key == hash);
    }

    [Test]
    public async Task CreateAsync_ReturnsOpaqueId_AndStoresOnlyItsHash()
    {
        var user = Context.BuildTestUser();
        var sessionId = await _service.CreateAsync(user.Id, "agent");

        await Assert.That(sessionId.Length).IsEqualTo(AuthenticationStaticOptions.SessionIdLength);
        await Assert.That(sessionId.All(char.IsLetterOrDigit)).IsTrue();

        var stored = await GetStoredAsync(sessionId);
        await Assert.That(stored.Key).IsNotEqualTo(sessionId);
        await Assert.That(stored.Key).IsEqualTo(Session.Hash(sessionId));
        await Assert.That(stored.UserId).IsEqualTo(user.Id);
        await Assert.That(stored.UserAgent).IsEqualTo("agent");
    }

    [Test]
    public async Task CreateAsync_NeverReturnsTheSameIdTwice()
    {
        var user = Context.BuildTestUser();
        var ids = new HashSet<string>();
        for (var i = 0; i < 20; i++)
            ids.Add(await _service.CreateAsync(user.Id, "agent"));

        await Assert.That(ids.Count).IsEqualTo(20);
    }

    [Test]
    public async Task CreateAsync_SetsBothIdleAndAbsoluteDeadlines()
    {
        var user = Context.BuildTestUser();
        var sessionId = await _service.CreateAsync(user.Id, "agent");
        var stored = await GetStoredAsync(sessionId);

        await Assert.That(stored.ExpiredAt).IsBetween(
            DateTime.UtcNow.AddMilliseconds(IdleMs).AddMinutes(-1),
            DateTime.UtcNow.AddMilliseconds(IdleMs).AddMinutes(1));
        await Assert.That(stored.AbsoluteExpiredAt).IsBetween(
            DateTime.UtcNow.AddMilliseconds(AbsoluteMs).AddMinutes(-1),
            DateTime.UtcNow.AddMilliseconds(AbsoluteMs).AddMinutes(1));
    }

    [Test]
    public async Task AuthorizeAsync_WithValidSession_ReturnsTheUser()
    {
        var user = Context.BuildTestUser();
        var sessionId = await _service.CreateAsync(user.Id, "agent");

        var result = await _service.AuthorizeAsync(sessionId);

        await Assert.That(result.IsAuthorized).IsTrue();
        await Assert.That(result.UserId).IsEqualTo(user.Id);
        await Assert.That(result.Scheme).IsEqualTo(AuthorizeType.Session);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    [Arguments("unknown-session-id")]
    public async Task AuthorizeAsync_WithMissingOrUnknownId_IsNotAuthorized(string? sessionId)
    {
        var result = await _service.AuthorizeAsync(sessionId);

        await Assert.That(result.IsAuthorized).IsFalse();
        await Assert.That(result.Scheme).IsNull();
    }

    [Test]
    public async Task AuthorizeAsync_WithIdleExpiredSession_IsNotAuthorized()
    {
        var user = Context.BuildTestUser();
        var sessionId = await _service.CreateAsync(user.Id, "agent");
        await ForceDeadlinesAsync(sessionId, DateTime.UtcNow.AddMinutes(-1), null);

        var result = await _service.AuthorizeAsync(sessionId);

        await Assert.That(result.IsAuthorized).IsFalse();
    }

    [Test]
    public async Task AuthorizeAsync_WithAbsoluteExpiredSession_IsNotAuthorized_EvenWhenRecentlyUsed()
    {
        var user = Context.BuildTestUser();
        var sessionId = await _service.CreateAsync(user.Id, "agent");
        // Idle deadline far in the future: only the absolute one can reject it.
        await ForceDeadlinesAsync(sessionId, DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddMinutes(-1));

        var result = await _service.AuthorizeAsync(sessionId);

        await Assert.That(result.IsAuthorized).IsFalse();
    }

    [Test]
    public async Task AuthorizeAsync_OnExpiredSession_DeletesItFromTheDatabase()
    {
        var user = Context.BuildTestUser();
        var sessionId = await _service.CreateAsync(user.Id, "agent");
        await ForceDeadlinesAsync(sessionId, DateTime.UtcNow.AddMinutes(-1), null);

        await _service.AuthorizeAsync(sessionId);

        var hash = Session.Hash(sessionId);
        await Assert.That(await Context.Sessions.AnyAsync(s => s.Key == hash)).IsFalse();
    }

    [Test]
    public async Task AuthorizeAsync_WithInactiveUser_IsNotAuthorized()
    {
        var user = Context.BuildTestUser(new TestUserPayload { IsActive = false });
        var sessionId = await _service.CreateAsync(user.Id, "agent");

        var result = await _service.AuthorizeAsync(sessionId);

        await Assert.That(result.IsAuthorized).IsFalse();
    }

    [Test]
    public async Task AuthorizeAsync_SlidesExpiration_WhenThresholdElapsed()
    {
        var user = Context.BuildTestUser();
        var sessionId = await _service.CreateAsync(user.Id, "agent");
        // Rewind the idle deadline past the renewal threshold, as if the session had been idle a while.
        var staleDeadline = DateTime.UtcNow.AddMilliseconds(IdleMs)
            .AddMilliseconds(-AuthenticationStaticOptions.SessionRenewalThresholdMs - 60000);
        await ForceDeadlinesAsync(sessionId, staleDeadline, null);

        await _service.AuthorizeAsync(sessionId);

        var stored = await GetStoredAsync(sessionId);
        await Assert.That(stored.ExpiredAt).IsGreaterThan(staleDeadline);
        await Assert.That(stored.UpdatedAt).IsNotNull();
    }

    [Test]
    public async Task AuthorizeAsync_DoesNotWrite_WhenBelowThreshold()
    {
        var user = Context.BuildTestUser();
        var sessionId = await _service.CreateAsync(user.Id, "agent");
        var before = await GetStoredAsync(sessionId);

        for (var i = 0; i < 5; i++)
            await _service.AuthorizeAsync(sessionId);

        var after = await GetStoredAsync(sessionId);
        await Assert.That(after.ExpiredAt).IsEqualTo(before.ExpiredAt);
        await Assert.That(after.UpdatedAt).IsEqualTo(before.UpdatedAt);
    }

    [Test]
    public async Task AuthorizeAsync_ClampsTheSlide_OnTheAbsoluteDeadline()
    {
        var user = Context.BuildTestUser();
        var sessionId = await _service.CreateAsync(user.Id, "agent");
        // Absolute deadline nearer than a full idle window: the slide must not go past it.
        var absolute = DateTime.UtcNow.AddMinutes(30);
        await ForceDeadlinesAsync(sessionId, DateTime.UtcNow.AddMinutes(1), absolute);

        await _service.AuthorizeAsync(sessionId);

        var stored = await GetStoredAsync(sessionId);
        await Assert.That(stored.ExpiredAt).IsLessThanOrEqualTo(absolute);
    }

    [Test]
    public async Task RevokeAsync_RemovesOnlyThatSession()
    {
        var user = Context.BuildTestUser();
        var revoked = await _service.CreateAsync(user.Id, "agent");
        var kept = await _service.CreateAsync(user.Id, "agent");

        await _service.RevokeAsync(revoked);

        await Assert.That((await _service.AuthorizeAsync(revoked)).IsAuthorized).IsFalse();
        await Assert.That((await _service.AuthorizeAsync(kept)).IsAuthorized).IsTrue();
    }

    [Test]
    public async Task RevokeAllAsync_RemovesEverySessionOfTheUser()
    {
        var user = Context.BuildTestUser();
        var other = Context.BuildTestUser();
        var first = await _service.CreateAsync(user.Id, "agent");
        var second = await _service.CreateAsync(user.Id, "agent");
        var untouched = await _service.CreateAsync(other.Id, "agent");

        await _service.RevokeAllAsync(user.Id);

        await Assert.That((await _service.AuthorizeAsync(first)).IsAuthorized).IsFalse();
        await Assert.That((await _service.AuthorizeAsync(second)).IsAuthorized).IsFalse();
        await Assert.That((await _service.AuthorizeAsync(untouched)).IsAuthorized).IsTrue();
    }

    [Test]
    public async Task CreateAsync_EvictsTheLeastRecentlyUsedSession_BeyondTheCap()
    {
        var user = Context.BuildTestUser();
        var max = AuthenticationStaticOptions.MaxConcurrentSessions;
        var sessions = new List<string>();
        for (var i = 0; i < max; i++)
            sessions.Add(await _service.CreateAsync(user.Id, "agent"));

        // Make the first one the least recently used, the others more recent.
        await TouchAsync(sessions[0], DateTime.UtcNow.AddHours(-3));
        for (var i = 1; i < max; i++)
            await TouchAsync(sessions[i], DateTime.UtcNow.AddMinutes(-i));

        await _service.CreateAsync(user.Id, "agent");

        await Assert.That(await Context.Sessions.CountAsync(s => s.UserId == user.Id)).IsEqualTo(max);
        await Assert.That((await _service.AuthorizeAsync(sessions[0])).IsAuthorized).IsFalse();
        await Assert.That((await _service.AuthorizeAsync(sessions[1])).IsAuthorized).IsTrue();
    }

    private async Task ForceDeadlinesAsync(string sessionId, DateTime? expiredAt, DateTime? absoluteExpiredAt)
    {
        var hash = Session.Hash(sessionId);
        var query = Context.Sessions.Where(s => s.Key == hash);
        if (expiredAt is not null)
            await query.ExecuteUpdateAsync(s => s.SetProperty(x => x.ExpiredAt, expiredAt.Value));
        if (absoluteExpiredAt is not null)
            await query.ExecuteUpdateAsync(s => s.SetProperty(x => x.AbsoluteExpiredAt, absoluteExpiredAt.Value));
    }

    private async Task TouchAsync(string sessionId, DateTime updatedAt)
    {
        var hash = Session.Hash(sessionId);
        await Context.Sessions
            .Where(s => s.Key == hash)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedAt, updatedAt));
    }
}