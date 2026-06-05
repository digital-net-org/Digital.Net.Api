using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Cms.Http.Dto;

namespace Digital.Net.Tests.Core.Sdk;

public static class PageApi
{
    public const string BaseUrl = "/cms/pages";

    public static async Task<HttpResponseMessage> GetPageById(this HttpClient client, Guid pageId) =>
        await client.GetAsync($"{BaseUrl}/{pageId}");

    public static async Task<HttpResponseMessage> CreatePage(this HttpClient client, PagePayload payload) =>
        await client.PostAsJsonAsync(BaseUrl, payload);

    public static async Task<HttpResponseMessage> PatchPage(this HttpClient client, Guid pageId, object patch) =>
        await client.PatchAsJsonAsync($"{BaseUrl}/{pageId}", patch);

    public static async Task<HttpResponseMessage> GetPublicPageByPath(this HttpClient client, string path) =>
        await client.GetAsync($"{BaseUrl}/public/path?path={Uri.EscapeDataString(path)}");

    public static async Task<HttpResponseMessage> GetPathAvailability(
        this HttpClient client,
        string path,
        Guid? excludeId = null
    )
    {
        var url = $"{BaseUrl}/path/availability?path={Uri.EscapeDataString(path)}";
        if (excludeId.HasValue) url += $"&excludeId={excludeId.Value}";
        return await client.GetAsync(url);
    }

    public static async Task<HttpResponseMessage> GetPageSheets(this HttpClient client, Guid pageId) =>
        await client.GetAsync($"{BaseUrl}/{pageId}/sheets");

    public static async Task<HttpResponseMessage> GetPageOpenGraph(this HttpClient client, Guid pageId) =>
        await client.GetAsync($"{BaseUrl}/{pageId}/open-graph");
}
