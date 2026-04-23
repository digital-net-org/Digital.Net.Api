using System.Text.Json;
using Digital.Net.Cms.Models;
using Digital.Net.Core.Services.Crud.Exceptions;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Cms.Services.Pages;

public static class PageValidator
{
    /// <summary>
    ///     Ensures that an EntityType can only be set when the path contains at least one
    ///     dynamic slug (e.g. <c>:id</c>). Returns an empty <see cref="Result" /> on success.
    /// </summary>
    public static Result EnsureEntityTypeConsistency(string path, PageEntityType? entityType)
    {
        var result = new Result();
        if (entityType is not null && !PagePathAnalyzer.HasDynamicSlug(path))
            result.AddError(
                new EntityValidationException(
                    "EntityType: This field requires at least one dynamic slug (:xxx) in the path."
                ));

        return result;
    }

    /// <summary>
    ///     Validates a JSON Patch against an existing page by simulating the post-patch state
    ///     of Path and EntityType, then checking consistency.
    /// </summary>
    public static Result ValidatePatch(JsonElement patch, Page current)
    {
        var nextPath = TryGetPatchValue(patch, nameof(Page.Path), out var pathValue)
            ? pathValue.GetString() ?? current.Path
            : current.Path;

        var nextEntityType = current.EntityType;
        if (TryGetPatchValue(patch, nameof(Page.EntityType), out var etValue))
            nextEntityType = etValue.ValueKind == JsonValueKind.Null
                ? null
                : JsonSerializer.Deserialize<PageEntityType?>(etValue.GetRawText());

        return EnsureEntityTypeConsistency(nextPath, nextEntityType);
    }

    private static bool TryGetPatchValue(JsonElement patch, string propertyName, out JsonElement value)
    {
        value = default;
        if (patch.ValueKind != JsonValueKind.Array)
            return false;
        foreach (var op in patch.EnumerateArray())
        {
            if (!op.TryGetProperty("path", out var pathEl)) continue;
            if (pathEl.GetString()?.Equals($"/{propertyName}", StringComparison.OrdinalIgnoreCase) != true) continue;
            if (!op.TryGetProperty("value", out value)) return false;
            return true;
        }

        return false;
    }
}