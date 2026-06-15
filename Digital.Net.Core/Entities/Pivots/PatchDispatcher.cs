using System.Text.Json;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Core.Entities.Pivots;

public class PatchDispatcher<TParent>(IEnumerable<IPivotPatchResolver<TParent>> resolvers)
    where TParent : Entity
{
    private readonly Dictionary<string, IPivotPatchResolver<TParent>> _resolvers = resolvers
        .ToDictionary(r => r.VirtualPath, StringComparer.OrdinalIgnoreCase);

    private readonly List<(IPivotPatchResolver<TParent> Resolver, JsonElement Value)> _pending = [];

    public (IReadOnlyList<JsonElement> SanitizedOps, Result Validation) ExtractAndValidate(
        JsonElement patch,
        Guid parentId
    )
    {
        _pending.Clear();
        var validation = new Result();
        var remaining = new List<JsonElement>();
        if (patch.ValueKind != JsonValueKind.Array)
            return (remaining, validation);

        foreach (var op in patch.EnumerateArray())
        {
            if (!TryMatchResolver(op, out var resolver) || !op.TryGetProperty("value", out var valueEl))
            {
                // Kept verbatim as a System.Text.Json element; serialized to text only once, downstream.
                remaining.Add(op);
                continue;
            }

            var opValidation = resolver!.ValidateValue(valueEl, parentId);
            validation.Merge(opValidation);
            // Pivot ops are applied later (across awaits), so clone them off the request document.
            _pending.Add((resolver, valueEl.Clone()));
        }

        return (remaining, validation);
    }

    public async Task ApplyPendingAsync(Guid parentId, CancellationToken ct)
    {
        foreach (var (resolver, value) in _pending)
            await resolver.ApplyAsync(value, parentId, ct);
        _pending.Clear();
    }

    private bool TryMatchResolver(JsonElement op, out IPivotPatchResolver<TParent>? resolver)
    {
        resolver = null;
        if (!op.TryGetProperty("path", out var pathEl)) return false;
        var path = pathEl.GetString();
        if (string.IsNullOrEmpty(path)) return false;
        return _resolvers.TryGetValue(path, out resolver);
    }
}
