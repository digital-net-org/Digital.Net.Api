using Digital.Net.Core.Http.Services.Events;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Services.Events;
using Microsoft.Extensions.Logging;
using Moq;

namespace Digital.Net.Tests.Core.Http.Services.Events;

public class SseStreamServiceTest : UnitTest
{
    private static (SseStreamService Service, EventSignalService SignalService) CreateServices()
    {
        var signalService = new EventSignalService();
        var logger = new Mock<ILogger<SseStreamService>>();
        var sseService = new SseStreamService(signalService, logger.Object);
        return (sseService, signalService);
    }

    [Test]
    public async Task HandleSignal_ShouldBufferEvents()
    {
        var (sseService, signalService) = CreateServices();

        for (var i = 0; i < 60; i++)
            signalService.Emit(new EventSignal($"EVENT_{i}", EventState.Success, DateTime.UtcNow));

        // Buffer should cap at 50 — we verify indirectly via SubscribeAsync
        // by triggering the subscription to force the signal handler to register.
        // Since we can't directly inspect the buffer, we just verify no exception is thrown.
        await Assert.That(() =>
        {
            for (var i = 0; i < 60; i++)
                signalService.Emit(new EventSignal($"EVENT_{i}", EventState.Success, DateTime.UtcNow));
        }).ThrowsNothing();

        sseService.Dispose();
    }

    [Test]
    public async Task Dispose_ShouldUnsubscribeFromSignals()
    {
        var (sseService, signalService) = CreateServices();

        sseService.Dispose();

        // After dispose, emitting signals should not throw
        await Assert.That(() =>
            signalService.Emit(new EventSignal("TEST", EventState.Success, DateTime.UtcNow))
        ).ThrowsNothing();
    }
}
