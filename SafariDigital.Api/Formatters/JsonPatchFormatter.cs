using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace SafariDigital.Api.Formatters;

public static class JsonPatchFormatter
{
    public static JsonPatchDocument<T> GetPatchDocument<T>(JsonElement patch) where T : class =>
        JsonConvert.DeserializeObject<JsonPatchDocument<T>>(patch.GetRawText())
        ?? new JsonPatchDocument<T>();
}