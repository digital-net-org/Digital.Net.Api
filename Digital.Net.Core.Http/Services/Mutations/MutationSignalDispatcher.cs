using Digital.Net.Lib.Entities.Mutations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Core.Http.Services.Mutations;

public sealed class MutationSignalDispatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<MutationSignalDispatcher> logger
)
{
    public async Task DispatchAsync(MutationSignal signal, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        foreach (var handler in scope.ServiceProvider.GetServices<IMutationSignalHandler>())
            try
            {
                await handler.HandleAsync(signal, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Mutation handler {Handler} failed for {EntityType} {EntityId}",
                    handler.GetType().Name, signal.EntityType, signal.EntityId);
            }
    }
}
