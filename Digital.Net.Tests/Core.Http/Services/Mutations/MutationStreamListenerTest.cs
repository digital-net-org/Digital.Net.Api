using System.Text;
using Digital.Net.Core;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Mutations;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Core.Http.Services.Mutations;
using Digital.Net.Tests.Core.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Digital.Net.Tests.Core.Http.Services.Mutations;

public class MutationStreamListenerTest
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    [Test]
    public async Task Listener_FansOutNotifiedSignal_ToConnectedClient()
    {
        var expectedId = Guid.NewGuid();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                    { [CoreSettings.ConnectionStringKey] = DbFixture.Fixture.ConnectionString }
            )
            .Build();

        var sse = new SseStreamService(NullLogger<SseStreamService>.Instance);
        var body = new MemoryStream();
        var httpContext = new DefaultHttpContext { Response = { Body = body } };
        using var clientCts = new CancellationTokenSource();
        var streaming = sse.StreamAsync(
            httpContext, null, _ => Task.FromResult<IReadOnlyList<MutationSignal>>([]), clientCts.Token);

        var provider = new ServiceCollection()
            .AddSingleton(sse)
            .AddScoped<IMutationSignalHandler, SseBroadcastHandler>()
            .BuildServiceProvider();
        var dispatcher = new MutationSignalDispatcher(
            provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<MutationSignalDispatcher>.Instance);
        var listener = new MutationStreamListener(dispatcher, config, NullLogger<MutationStreamListener>.Instance);
        await listener.StartAsync(CancellationToken.None);
        await Task.Delay(750, clientCts.Token);

        var signal = new MutationSignal(ChangeType.Updated, "Page", expectedId, DateTime.UtcNow, Guid.NewGuid());
        await using (var ctx = DbFixture.CreateContext<DigitalContext>())
            await new MutationBroadcaster(NullLogger<MutationBroadcaster>.Instance)
                .PublishAsync(ctx, [signal], CancellationToken.None);

        await Task.Delay(500, clientCts.Token);

        await clientCts.CancelAsync();
        try { await streaming; }
        catch (OperationCanceledException) { }
        await listener.StopAsync(CancellationToken.None);

        var output = Encoding.UTF8.GetString(body.ToArray());
        await Assert.That(output.Contains("event: mutation")).IsTrue();
        await Assert.That(output.Contains(expectedId.ToString())).IsTrue();
    }
}
