namespace Digital.Net.Core.Services.Events;

public interface IEventSignalService
{
    event Action<EventSignal>? OnSignal;
    void Emit(EventSignal signal);
}
