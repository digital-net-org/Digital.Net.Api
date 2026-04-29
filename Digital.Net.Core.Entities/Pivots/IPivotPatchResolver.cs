using System.Text.Json;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Core.Entities.Pivots;

/// <summary>
///     Resolves pivot operations for parent type <typeparamref name="T" />. The generic parameter is a DI discriminator:
///     the container resolves IEnumerable&lt;IPivotPatchResolver&lt;TParent&gt;&gt; to scope dispatchers to a single type.
/// </summary>
public interface IPivotPatchResolver<T>
{
    string VirtualPath { get; }
    Result ValidateValue(JsonElement value, Guid parentId);
    Task ApplyAsync(JsonElement value, Guid parentId, CancellationToken ct);
}
