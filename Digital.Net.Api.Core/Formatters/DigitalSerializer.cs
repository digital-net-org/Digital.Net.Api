using System.Text.Json;

namespace Digital.Net.Api.Core.Formatters;

public class DigitalSerializer
{
    private static readonly JsonSerializerOptions SerializerConfig =
        new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            MaxDepth = 64,
        };

    public static T Deserialize<T>(string json)
    {
        var deserializedJson = JsonSerializer.Deserialize<T>(json.Length == 0 ? "{}" : json, SerializerConfig);
        return deserializedJson is null ? throw new Exception("Failed to deserialize JSON") : deserializedJson;
    }

    public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, SerializerConfig);
}