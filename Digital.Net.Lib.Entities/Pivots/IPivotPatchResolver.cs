using System.Text.Json;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Lib.Entities.Pivots;

public interface IPivotPatchResolver<T>
{
    string VirtualPath { get; }
    Result ValidateValue(JsonElement value, Guid parentId);
    Task ApplyAsync(JsonElement value, Guid parentId, CancellationToken ct);
}
