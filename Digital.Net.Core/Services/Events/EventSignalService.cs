namespace Digital.Net.Core.Services.Events;

public class EventSignalService : IEventSignalService
{
    public event Action<EventSignal>? OnSignal;
    public void Emit(EventSignal signal) => OnSignal?.Invoke(signal);
}
