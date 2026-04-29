using System.Text.Json.Serialization;

namespace Digital.Net.Cms.Models.Pages;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PageEntityType
{
    Article,
    Form
}
