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

    private static (SseStreamService, HttpContext, MemoryStream, CancellationTokenSource) Setup(
        IReadOnlySet<string>? entityTypes)
    {
        var service = new SseStreamService(NullLogger<SseStreamService>.Instance);
        var body = new MemoryStream();
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = body;
        return (service, ctx, body, new CancellationTokenSource());
    }
}