using Digital.Net.Lib.Entities.Mutations;
using Digital.Net.Core.Http.Services.Mutations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Digital.Net.Tests.Core.Http.Services.Mutations;

public class MutationSignalDispatcherTest
{
    private static MutationSignal Signal() =>
        new(ChangeType.Updated, "Page", Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid());

    private static MutationSignalDispatcher BuildDispatcher(params IMutationSignalHandler[] handlers)
    {
        var services = new ServiceCollection();
        foreach (var handler in handlers)
            services.AddSingleton(handler);
        var provider = services.BuildServiceProvider();
        return new MutationSignalDispatcher(
            provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<MutationSignalDispatcher>.Instance);
    }

    [Test]
    public async Task DispatchAsync_InvokesEveryRegisteredHandler()
    {
        var first = new RecordingHandler();
        var second = new RecordingHandler();
        var dispatcher = BuildDispatcher(first, second);

        var signal = Signal();
        await dispatcher.DispatchAsync(signal, CancellationToken.None);

        await Assert.That(first.Received.Count).IsEqualTo(1);
        await Assert.That(second.Received.Count).IsEqualTo(1);
        await Assert.That(first.Received[0].EntityId).IsEqualTo(signal.EntityId);
    }

    [Test]
    public async Task DispatchAsync_IsolatesThrowingHandler_OthersStillRun()
    {
        var throwing = new ThrowingHandler();
        var recording = new RecordingHandler();
        var dispatcher = BuildDispatcher(throwing, recording);

        await dispatcher.DispatchAsync(Signal(), CancellationToken.None);

        await Assert.That(throwing.WasCalled).IsTrue();
        await Assert.That(recording.Received.Count).IsEqualTo(1);
    }

    private sealed class RecordingHandler : IMutationSignalHandler
    {
        public List<MutationSignal> Received { get; } = [];

        public Task HandleAsync(MutationSignal signal, CancellationToken cancellationToken)
        {
            Received.Add(signal);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler : IMutationSignalHandler
    {
        public bool WasCalled { get; private set; }

        public Task HandleAsync(MutationSignal signal, CancellationToken cancellationToken)
        {
            WasCalled = true;
            throw new InvalidOperationException("boom");
        }
    }
}
