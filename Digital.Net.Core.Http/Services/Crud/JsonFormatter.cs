using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Digital.Net.Core.Http.Services.Crud;

public static class JsonFormatter
{
    public static JsonPatchDocument<T> GetPatchDocument<T>(this IReadOnlyList<JsonElement> ops) where T : class =>
        JsonConvert.DeserializeObject<JsonPatchDocument<T>>(JsonSerializer.Serialize(ops))
        ?? new JsonPatchDocument<T>();
}