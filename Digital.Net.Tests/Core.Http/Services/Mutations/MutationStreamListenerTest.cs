using System.Text;
using Digital.Net.Core;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Mutations;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Core.Http.Services.Mutations;
using Digital.Net.Tests.Core.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Digital.Net.Tests.Core.Http.Services.Mutations;

/// <summary>
///     Proves the multi-pod transport end-to-end: a <c>pg_notify</c> issued on one connection (the broadcaster) is
///     received by the pod's dedicated <c>LISTEN</c> connection and fanned out to a connected SSE client — the real
///     Postgres wire, with the concrete <see cref="SseStreamService" /> (no test interface).
/// </summary>
public class MutationStreamListenerTest
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    [Test]
    public async Task Listener_FansOutNotifiedSignal_ToConnectedClient()
    {
        // Unique id: the shared test DB carries other tests' NOTIFYs on the same channel.
        var expectedId = Guid.NewGuid();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
                { [CoreSettings.ConnectionStringKey] = DbFixture.Fixture.ConnectionString })
            .Build();

        var sse = new SseStreamService(NullLogger<SseStreamService>.Instance);

        // Connect a real client to the concrete fan-out service (no catch-up).
        var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = body;
        using var clientCts = new CancellationTokenSource();
        var streaming = sse.StreamAsync(
            httpContext, null, _ => Task.FromResult<IReadOnlyList<MutationSignal>>([]), clientCts.Token);

        var listener = new MutationStreamListener(sse, config, NullLogger<MutationStreamListener>.Instance);
        await listener.StartAsync(CancellationToken.None);
        await Task.Delay(750); // let the LISTEN connection establish before notifying

        var signal = new MutationSignal(ChangeType.Updated, "Page", expectedId, DateTime.UtcNow, Guid.NewGuid());
        await using (var ctx = DbFixture.CreateContext<DigitalContext>())
            await new MutationBroadcaster(NullLogger<MutationBroadcaster>.Instance)
                .PublishAsync(ctx, [signal], CancellationToken.None);

        await Task.Delay(500); // NOTIFY → listener → fan-out → client write

        await clientCts.CancelAsync(); // stop the client write loop before reading the buffer
        try { await streaming; }
        catch (OperationCanceledException) { }
        await listener.StopAsync(CancellationToken.None);

        var output = Encoding.UTF8.GetString(body.ToArray());
        await Assert.That(output.Contains("event: mutation")).IsTrue();
        await Assert.That(output.Contains(expectedId.ToString())).IsTrue();
    }
}
