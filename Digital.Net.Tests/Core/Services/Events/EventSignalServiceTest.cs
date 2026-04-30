using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Services.Events;

namespace Digital.Net.Tests.Core.Services.Events;

public class EventSignalServiceTest : UnitTest
{
    [Test]
    public async Task Emit_ShouldInvokeOnSignal()
    {
        var service = new EventSignalService();
        EventSignal? received = null;
        service.OnSignal += signal => received = signal;

        var expected = new EventSignal("TEST_EVENT", EventState.Success, DateTime.UtcNow);
        service.Emit(expected);

        await Assert.That(received).IsNotNull();
        await Assert.That(received!.Name).IsEqualTo("TEST_EVENT");
        await Assert.That(received.State).IsEqualTo(EventState.Success);
    }

    [Test]
    public async Task Emit_ShouldNotThrow_WhenNoSubscribers()
    {
        var service = new EventSignalService();
        var signal = new EventSignal("TEST_EVENT", EventState.Success, DateTime.UtcNow);

        var action = () => service.Emit(signal);

        await Assert.That(action).ThrowsNothing();
    }
}
