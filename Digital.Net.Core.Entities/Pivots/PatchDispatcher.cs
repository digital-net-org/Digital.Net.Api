using System.Text.Json;
using System.Text.Json.Nodes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Core.Entities.Pivots;

public class PatchDispatcher<TParent>(IEnumerable<IPivotPatchResolver<TParent>> resolvers)
    where TParent : Entity
{
    private readonly Dictionary<string, IPivotPatchResolver<TParent>> _resolvers = resolvers
        .ToDictionary(r => r.VirtualPath, StringComparer.OrdinalIgnoreCase);

    private readonly List<(IPivotPatchResolver<TParent> Resolver, JsonElement Value)> _pending = [];

    /// <summary>
    ///     Walks <paramref name="patch"/>, extracts ops whose <c>path</c> matches a registered resolver,
    ///     validates their <c>value</c>, and returns the remainder so it can be fed to the parent patch.
    /// </summary>
    public (JsonElement SanitizedPatch, Result Validation) ExtractAndValidate(JsonElement patch, Guid parentId)
    {
        _pending.Clear();
        var validation = new Result();
        if (patch.ValueKind != JsonValueKind.Array)
            return (patch, validation);

        var remaining = new JsonArray();
        foreach (var op in patch.EnumerateArray())
        {
            if (!TryMatchResolver(op, out var resolver) || !op.TryGetProperty("value", out var valueEl))
            {
                remaining.Add(JsonNode.Parse(op.GetRawText()));
                continue;
            }

            var opValidation = resolver!.ValidateValue(valueEl, parentId);
            validation.Merge(opValidation);
            _pending.Add((resolver, valueEl.Clone()));
        }

        return (JsonSerializer.SerializeToElement(remaining), validation);
    }

    /// <summary>Applies every op that <see cref="ExtractAndValidate"/> has captured.</summary>
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
