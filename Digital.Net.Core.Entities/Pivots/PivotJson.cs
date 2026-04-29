using System.Text.Json;

namespace Digital.Net.Core.Entities.Pivots;

public static class PivotJson
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static List<T> Deserialize<T>(JsonElement value)
        where T : class
        => value.Deserialize<List<T>>(JsonOptions) ?? [];
}