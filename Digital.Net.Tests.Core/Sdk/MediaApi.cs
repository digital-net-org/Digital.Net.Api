using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Tests.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class MediaApi
{
    public const string BaseUrl = "/cms/media";

    public static async Task<HttpResponseMessage> GetMediaById(this HttpClient client, Guid mediaId) =>
        await client.GetAsync($"{BaseUrl}/{mediaId}");

    public static async Task<HttpResponseMessage> GetMedia(this HttpClient client, MediaQuery? query = null) =>
        await client.GetAsync($"{BaseUrl}{query?.ToQueryString()}");

    public static async Task<HttpResponseMessage> UploadMedia(
        this HttpClient client,
        byte[] fileBytes,
        string fileName,
        string contentType,
        string name,
        string? alt = null
    )
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(name), "name");
        if (alt is not null)
            content.Add(new StringContent(alt), "alt");

        return await client.PostAsync(BaseUrl, content);
    }

    public static async Task<HttpResponseMessage> PatchMedia(this HttpClient client, Guid mediaId, object patch) =>
        await client.PatchAsJsonAsync($"{BaseUrl}/{mediaId}", patch);

    public static async Task<HttpResponseMessage> DeleteMedia(this HttpClient client, Guid mediaId) =>
        await client.DeleteAsync($"{BaseUrl}/{mediaId}");

    public static async Task<HttpResponseMessage> PurgeMediaVariants(this HttpClient client, Guid mediaId) =>
        await client.DeleteAsync($"{BaseUrl}/{mediaId}/variants");

    public static async Task<HttpResponseMessage> PurgeVariant(this HttpClient client, Guid variantId) =>
        await client.DeleteAsync($"{BaseUrl}/variants/{variantId}");

    public static async Task<HttpResponseMessage> PurgeAllVariants(this HttpClient client) =>
        await client.DeleteAsync($"{BaseUrl}/variants");

    public static async Task<HttpResponseMessage> GetMediaImage(
        this HttpClient client,
        Guid mediaId,
        string ext,
        int? w = null,
        int? q = null
    )
    {
        var url = $"{BaseUrl}/image/{mediaId}.{ext}";
        var queryParams = new List<string>();
        if (w.HasValue) queryParams.Add($"w={w.Value}");
        if (q.HasValue) queryParams.Add($"q={q.Value}");
        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

        return await client.GetAsync(url);
    }
}
