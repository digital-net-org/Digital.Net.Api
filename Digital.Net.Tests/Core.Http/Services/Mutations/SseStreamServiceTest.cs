using System.Text;
using Digital.Net.Core.Entities.Models.Mutations;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Core.Http.Services.Mutations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Digital.Net.Tests.Core.Http.Services.Mutations;

public class SseStreamServiceTest
{
    private static readonly IReadOnlyList<MutationSignal> NoCatchUp = [];

    [Test]
    public async Task Broadcast_WritesMatchingSignalAsSseFrame()
    {
        var (service, ctx, body, cts) = Setup(null);
        var streaming = service.StreamAsync(ctx, null, _ => Task.FromResult(NoCatchUp), cts.Token);

        await Task.Delay(150);
        var signal = new MutationSignal(ChangeType.Updated, "Page", Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid());
        service.Broadcast(signal);
        await Task.Delay(150);
        await cts.CancelAsync();
        await streaming;

        var output = Encoding.UTF8.GetString(body.ToArray());
        await Assert.That(output.Contains("event: mutation")).IsTrue();
        await Assert.That(output.Contains(signal.EntityId.ToString())).IsTrue();
    }

    [Test]
    public async Task Broadcast_SkipsSignalNotMatchingEntityTypeFilter()
    {
        var (service, ctx, body, cts) = Setup(new HashSet<string> { "Article" });
        var streaming = service.StreamAsync(ctx, new HashSet<string> { "Article" }, _ => Task.FromResult(NoCatchUp),
            cts.Token);

        await Task.Delay(150);
        service.Broadcast(new MutationSignal(ChangeType.Updated, "Page", Guid.NewGuid(), DateTime.UtcNow,
            Guid.NewGuid()));
        await Task.Delay(150);
        await cts.CancelAsync();
        await streaming;

        var output = Encoding.UTF8.GetString(body.ToArray());
        await Assert.That(output.Contains("Page")).IsFalse();
    }

    [Test]
    public async Task Broadcast_IncludesOriginClientId_WhenPresent()
    {
        // The frame carries the originating tab's client id verbatim — the client (not the server) decides
        // whether to drop its own echo.
        var (service, ctx, body, cts) = Setup(null);
        var streaming = service.StreamAsync(ctx, null, _ => Task.FromResult(NoCatchUp), cts.Token, Guid.NewGuid());

        await Task.Delay(150);
        service.Broadcast(new MutationSignal(ChangeType.Updated, "Page", Guid.NewGuid(), DateTime.UtcNow,
            Guid.NewGuid(), Guid.NewGuid(), "client-abc"));
        await Task.Delay(150);
        await cts.CancelAsync();
        await streaming;

        var output = Encoding.UTF8.GetString(body.ToArray());
        await Assert.That(output.Contains("\"originClientId\":\"client-abc\"")).IsTrue();
    }

    [Test]
    public async Task Broadcast_EmitsNullOriginClientId_WhenAbsent()
    {
        var (service, ctx, body, cts) = Setup(null);
        var streaming = service.StreamAsync(ctx, null, _ => Task.FromResult(NoCatchUp), cts.Token, Guid.NewGuid());

        await Task.Delay(150);
        service.Broadcast(new MutationSignal(ChangeType.Updated, "Page", Guid.NewGuid(), DateTime.UtcNow,
            Guid.NewGuid()));
        await Task.Delay(150);
        await cts.CancelAsync();
        await streaming;

        var output = Encoding.UTF8.GetString(body.ToArray());
        await Assert.That(output.Contains("\"originClientId\":null")).IsTrue();
    }

    [Test]
    public async Task StreamAsync_CapsConnectionsPerUser_WithoutCappingOtherUsers()
    {
        var service = new SseStreamService(NullLogger<SseStreamService>.Instance);
        var user = Guid.NewGuid();
        var ctsList = new List<CancellationTokenSource>();
        var streams = new List<Task>();

        // 10 concurrent streams for the same user — all accepted (200).
        for (var i = 0; i < 10; i++)
        {
            var (ctx, cts) = NewStream();
            ctsList.Add(cts);
            streams.Add(service.StreamAsync(ctx, null, _ => Task.FromResult(NoCatchUp), cts.Token, user));
            await Assert.That(ctx.Response.StatusCode).IsEqualTo(200);
        }

        // 11th for the same user — refused with 429, no stream opened.
        var (refusedCtx, refusedCts) = NewStream();
        await service.StreamAsync(refusedCtx, null, _ => Task.FromResult(NoCatchUp), refusedCts.Token, user);
        await Assert.That(refusedCtx.Response.StatusCode).IsEqualTo(429);

        // A different user is unaffected — accepted (the cap is per-user, never global).
        var (otherCtx, otherCts) = NewStream();
        ctsList.Add(otherCts);
        streams.Add(service.StreamAsync(otherCtx, null, _ => Task.FromResult(NoCatchUp), otherCts.Token, Guid.NewGuid()));
        await Assert.That(otherCtx.Response.StatusCode).IsEqualTo(200);

        // Freeing one of the user's slots lets a new connection for that user through again.
        await ctsList[0].CancelAsync();
        await streams[0];
        var (retryCtx, retryCts) = NewStream();
        ctsList.Add(retryCts);
        streams.Add(service.StreamAsync(retryCtx, null, _ => Task.FromResult(NoCatchUp), retryCts.Token, user));
        await Assert.That(retryCtx.Response.StatusCode).IsEqualTo(200);

        foreach (var cts in ctsList) await cts.CancelAsync();
        foreach (var stream in streams) await stream;
    }

    private static (SseStreamService, HttpContext, MemoryStream, CancellationTokenSource) Setup(
        IReadOnlySet<string>? entityTypes)
    {
        var service = new SseStreamService(NullLogger<SseStreamService>.Instance);
        var body = new MemoryStream();
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = body;
        return (service, ctx, body, new CancellationTokenSource());
    }

    private static (DefaultHttpContext, CancellationTokenSource) NewStream()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return (ctx, new CancellationTokenSource());
    }
}