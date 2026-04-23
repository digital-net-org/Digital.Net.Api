using System.Text.Json;
using System.Text.Json.Nodes;
using Digital.Net.Cms.Models;
using Digital.Net.Core.Services.Crud.Exceptions;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Cms.Services.Pages;

public static class PagePayloadValidator
{
    private const string OpenGraphPath = "/openGraph";
    
    /// <summary>
    ///     Ensures that an EntityType can only be set when the path contains at least one
    ///     dynamic slug (e.g. <c>:id</c>). Returns an empty <see cref="Result" /> on success.
    /// </summary>
    public static Result ValidateEntityTypeConsistency(string path, PageEntityType? entityType)
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
        var result = new Result();

        // Validates path "/path"
        // Simulating the post-patch state of Path and EntityType, then checking consistency.
        // TryGetPatchValue returns false for non-array patches, so we fall back to current values.
        var nextPath = TryGetPatchValue(patch, nameof(Page.Path), out var pathValue)
            ? pathValue.GetString() ?? current.Path
            : current.Path;

        var nextEntityType = current.EntityType;
        if (TryGetPatchValue(patch, nameof(Page.EntityType), out var etValue))
            nextEntityType = etValue.ValueKind == JsonValueKind.Null
                ? null
                : JsonSerializer.Deserialize<PageEntityType?>(etValue.GetRawText());

        result.Merge(ValidateEntityTypeConsistency(nextPath, nextEntityType));
        if (result.HasError)
            return result;

        // Validate path "/openGraph"
        if (!TryGetPatchValue(patch, nameof(Page.OpenGraph), out var openGraphValue))
            return result;
        if (openGraphValue.ValueKind == JsonValueKind.Null)
            return result;
        if (openGraphValue.ValueKind != JsonValueKind.Array)
            return result.AddError(new EntityValidationException(
                "OpenGraph: Value must be an array of { property, content } entries."
            ));

        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        var invalidKeys = new List<string>();
        var duplicateKeys = new List<string>();
        var index = -1;
        foreach (var entry in openGraphValue.EnumerateArray())
        {
            index++;
            if (entry.ValueKind != JsonValueKind.Object)
                return result.AddError(new EntityValidationException(
                    $"OpenGraph: Entry at index {index} must be an object with 'property' and 'content'."
                ));
            if (!TryGetString(entry, "property", out var property) || string.IsNullOrEmpty(property))
                return result.AddError(new EntityValidationException(
                    $"OpenGraph: Entry at index {index} is missing a non-empty string 'property'."
                ));
            if (!TryGetString(entry, "content", out var content))
                return result.AddError(new EntityValidationException(
                    $"OpenGraph: Entry at index {index} is missing a string 'content'."
                ));
            if (string.IsNullOrEmpty(content))
                return result.AddError(new EntityValidationException(
                    $"OpenGraph: Entry at index {index} has an empty 'content'."
                ));
            if (!OpenGraphProperties.ByKey.TryGetValue(property, out var schema))
            {
                if (!invalidKeys.Contains(property))
                    invalidKeys.Add(property);
                continue;
            }

            counts.TryGetValue(property, out var currentOgIndex);
            counts[property] = currentOgIndex + 1;
            if (!schema.AllowMultiple && counts[property] > 1 && !duplicateKeys.Contains(property))
                duplicateKeys.Add(property);
        }

        if (invalidKeys.Count > 0)
            return result.AddError(new EntityValidationException(
                $"OpenGraph: Unknown property keys: {string.Join(", ", invalidKeys)}."
            ));
        if (duplicateKeys.Count > 0)
            return result.AddError(new EntityValidationException(
                $"OpenGraph: Duplicate property keys not allowed: {string.Join(", ", duplicateKeys)}."
            ));

        return result;
    }
    
    /// <summary>
    ///     Rewrites ops targeting /openGraph so their <c>value</c> becomes the JSON-serialized string
    ///     of the entries array — matching the underlying <c>Page.OpenGraph: string?</c> column.
    ///     Non-matching ops are copied verbatim.
    /// </summary>
    public static JsonElement NormalizePatch(JsonElement patch)
    {
        if (patch.ValueKind != JsonValueKind.Array)
            return patch;

        var rewritten = new JsonArray();
        foreach (var op in patch.EnumerateArray())
        {
            var node = JsonNode.Parse(op.GetRawText())!.AsObject();
            if (IsOpenGraphReplaceValue(op, out var valueEl))
                node["value"] = valueEl.ValueKind == JsonValueKind.Null
                    ? null
                    : JsonValue.Create(JsonSerializer.Serialize(valueEl));
            rewritten.Add(node);
        }

        return JsonSerializer.SerializeToElement(rewritten);
    }

    private static bool IsOpenGraphReplaceValue(JsonElement op, out JsonElement valueEl)
    {
        valueEl = default;
        if (!op.TryGetProperty("path", out var pathEl)) return false;
        if (!string.Equals(pathEl.GetString(), OpenGraphPath, StringComparison.OrdinalIgnoreCase)) return false;
        return op.TryGetProperty("value", out valueEl);
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var prop)) return false;
        if (prop.ValueKind != JsonValueKind.String) return false;
        value = prop.GetString() ?? string.Empty;
        return true;
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