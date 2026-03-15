using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Core.Http;
using Digital.Net.Tests.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class TagApi
{
    public const string BaseUrl = "/cms/tags";

    public static async Task<HttpResponseMessage> GetTagById(this HttpClient client, Guid tagId) =>
        await client.GetAsync($"{BaseUrl}/{tagId}");

    public static async Task<HttpResponseMessage> GetTags(this HttpClient client, TagQuery? query = null) =>
        await client.GetAsync($"{BaseUrl}{query?.ToQueryString()}");

    public static async Task<HttpResponseMessage> CreateTag(this HttpClient client, TagPayload payload) =>
        await client.PostAsJsonAsync(BaseUrl, payload);

    public static async Task<HttpResponseMessage> PatchTag(this HttpClient client, Guid tagId, object patch) =>
        await client.PatchAsJsonAsync($"{BaseUrl}/{tagId}", patch);

    public static async Task<HttpResponseMessage> DeleteTag(this HttpClient client, Guid tagId) =>
        await client.DeleteAsync($"{BaseUrl}/{tagId}");
}
