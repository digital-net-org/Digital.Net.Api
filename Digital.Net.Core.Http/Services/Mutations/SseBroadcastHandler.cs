using Digital.Net.Lib.Entities.Mutations;

namespace Digital.Net.Core.Http.Services.Mutations;

public sealed class SseBroadcastHandler(SseStreamService sseStream) : IMutationSignalHandler
{
    public Task HandleAsync(MutationSignal signal, CancellationToken cancellationToken)
    {
        sseStream.Broadcast(signal);
        return Task.CompletedTask;
    }
}