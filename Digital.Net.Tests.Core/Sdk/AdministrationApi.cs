using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Tests.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class AdministrationApi
{
    public const string BaseUrl = "/user";

    public static async Task<HttpResponseMessage> GetUserById(this HttpClient client, Guid userId) =>
        await client.GetAsync($"{BaseUrl}/{userId}");

    public static async Task<HttpResponseMessage> GetUsers(this HttpClient client, UserQuery? query = null) =>
        await client.GetAsync($"{BaseUrl}{query?.ToQueryString()}");

    public static async Task<HttpResponseMessage> CreateUser(this HttpClient client, UserPayload payload) =>
        await client.PostAsJsonAsync(BaseUrl, payload);

    public static async Task<HttpResponseMessage> DeleteUser(this HttpClient client, Guid userId, UserDeletePayload payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{userId}")
        {
            Content = JsonContent.Create(payload)
        };
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> UpdateUserStatus(this HttpClient client, Guid userId, UserStatusPayload payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/{userId}/status")
        {
            Content = JsonContent.Create(payload)
        };
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> UpdateUserRole(this HttpClient client, Guid userId, UserRolePayload payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/{userId}/role")
        {
            Content = JsonContent.Create(payload)
        };
        return await client.SendAsync(request);
    }
}