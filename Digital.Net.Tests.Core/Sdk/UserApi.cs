using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Core.Http.Endpoints.Dto;

namespace Digital.Net.Tests.Core.Sdk;

public static class UserApi
{
    public const string BaseUrl = "/user";

    public static async Task<HttpResponseMessage> GetSelf(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}/self");

    public static async Task<HttpResponseMessage> UpdatePassword(
        this HttpClient client,
        string currentPassword,
        string newPassword
    ) =>
        await client.PutAsJsonAsync($"{BaseUrl}/self/password",
            new UserPasswordUpdatePayload { CurrentPassword = currentPassword, NewPassword = newPassword });

    public static async Task<HttpResponseMessage> UpdateAvatar(this HttpClient client, MultipartFormDataContent content) =>
        await client.PutAsync($"{BaseUrl}/self/avatar", content);

    public static async Task<HttpResponseMessage> RemoveAvatar(this HttpClient client) =>
        await client.DeleteAsync($"{BaseUrl}/self/avatar");

    public static async Task<HttpResponseMessage> GetUserAvatar(this HttpClient client, Guid userId) =>
        await client.GetAsync($"{BaseUrl}/{userId}/avatar");

    public static async Task<HttpResponseMessage> GetUserAvatar(this HttpClient client, Guid userId, string etag)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{userId}/avatar");
        request.Headers.TryAddWithoutValidation("If-None-Match", etag);
        return await client.SendAsync(request);
    }
}