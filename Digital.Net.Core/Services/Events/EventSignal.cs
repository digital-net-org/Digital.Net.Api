using Digital.Net.Core.Entities.Models.Events;

namespace Digital.Net.Core.Services.Events;

public record EventSignal(string Name, EventState State, DateTime OccurredAt);
