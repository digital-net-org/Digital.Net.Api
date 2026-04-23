using System.Text.Json.Serialization;

namespace Digital.Net.Cms.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PageEntityType
{
    Article,
    Form
}
