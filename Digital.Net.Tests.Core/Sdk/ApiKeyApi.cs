using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Api.Endpoints.Dto;

namespace Digital.Net.Tests.Core.Sdk;

public static class ApiKeyApi
{
    public const string BaseUrl = "/user/self/api-key";

    public static async Task<HttpResponseMessage> CreateApiKey(
        this HttpClient client,
        string name,
        DateTime? expiredAt = null
    ) =>
        await client.PostAsJsonAsync(BaseUrl, new ApiKeyCreatePayload { Name = name, ExpiredAt = expiredAt });

    public static async Task<HttpResponseMessage> ListApiKeys(this HttpClient client) =>
        await client.GetAsync(BaseUrl);

    public static async Task<HttpResponseMessage> DeleteApiKey(this HttpClient client, Guid id) =>
        await client.DeleteAsync($"{BaseUrl}/{id}");
}
