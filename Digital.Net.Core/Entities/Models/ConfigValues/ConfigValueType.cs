using System.Text.Json.Serialization;

namespace Digital.Net.Core.Entities.Models.ConfigValues;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConfigValueType
{
    String,
    Number,
    Boolean,
    Json
}