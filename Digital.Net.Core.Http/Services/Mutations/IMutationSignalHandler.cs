using Digital.Net.Core.Entities.Mutations;

namespace Digital.Net.Core.Http.Services.Mutations;

/// <summary>
///     A reactor to entity mutations. Every implementation registered in DI is invoked for each
///     <see cref="MutationSignal" /> received over the LISTEN/NOTIFY channel.
///     <para>
///         Implementations must be cheap and resilient: handlers run isolated (an exception is logged, never
///         rethrown).
///     </para>
/// </summary>
public interface IMutationSignalHandler
{
    Task HandleAsync(MutationSignal signal, CancellationToken cancellationToken);
}