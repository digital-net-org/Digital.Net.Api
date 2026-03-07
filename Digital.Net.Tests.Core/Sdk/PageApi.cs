using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Controllers.Dto;
using Digital.Net.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class PageApi
{
    public const string BaseUrl = "/page";

    public static async Task<HttpResponseMessage> GetPageById(this HttpClient client, Guid pageId) =>
        await client.GetAsync($"{BaseUrl}/{pageId}");

    public static async Task<HttpResponseMessage> GetPages(this HttpClient client, PageQuery? query = null) =>
        await client.GetAsync($"{BaseUrl}{query?.ToQueryString()}");

    public static async Task<HttpResponseMessage> CreatePage(this HttpClient client, PagePayload payload) =>
        await client.PostAsJsonAsync(BaseUrl, payload);

    public static async Task<HttpResponseMessage> PatchPage(this HttpClient client, Guid pageId, object patch) =>
        await client.PatchAsJsonAsync($"{BaseUrl}/{pageId}", patch);

    public static async Task<HttpResponseMessage> DeletePage(this HttpClient client, Guid pageId) =>
        await client.DeleteAsync($"{BaseUrl}/{pageId}");

    public static async Task<HttpResponseMessage> GetPageByPath(this HttpClient client, string path) =>
        await client.GetAsync($"{BaseUrl}/path/{path}");
}
