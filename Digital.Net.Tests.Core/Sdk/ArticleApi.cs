using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Tests.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class ArticleApi
{
    public const string BaseUrl = "/cms/articles";

    public static async Task<HttpResponseMessage> GetArticleById(this HttpClient client, Guid articleId) =>
        await client.GetAsync($"{BaseUrl}/{articleId}");

    public static async Task<HttpResponseMessage> GetArticles(this HttpClient client, ArticleQuery? query = null) =>
        await client.GetAsync($"{BaseUrl}{query?.ToQueryString()}");

    public static async Task<HttpResponseMessage> CreateArticle(this HttpClient client, ArticlePayload payload) =>
        await client.PostAsJsonAsync(BaseUrl, payload);

    public static async Task<HttpResponseMessage> GetSlugAvailability(
        this HttpClient client,
        string slug,
        Guid? excludeId = null
    )
    {
        var url = $"{BaseUrl}/slug/availability?slug={Uri.EscapeDataString(slug)}";
        if (excludeId.HasValue) url += $"&excludeId={excludeId.Value}";
        return await client.GetAsync(url);
    }
}
