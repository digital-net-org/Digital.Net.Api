using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class AdministrationApi
{
    public const string BaseUrl = "/admin";

    public static async Task<HttpResponseMessage> GetUserById(this HttpClient client, Guid userId) =>
        await client.GetAsync($"{BaseUrl}/user/{userId}");

    public static async Task<HttpResponseMessage> GetUsers(this HttpClient client, UserQuery? query = null) =>
        await client.GetAsync($"{BaseUrl}/user{query?.ToQueryString()}");

    public static async Task<HttpResponseMessage> CreateUser(this HttpClient client, UserPayload payload) =>
        await client.PostAsJsonAsync($"{BaseUrl}/user", payload);

    public static async Task<HttpResponseMessage> DeleteUser(this HttpClient client, Guid userId, UserDeletePayload payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/user/{userId}")
        {
            Content = JsonContent.Create(payload)
        };
        return await client.SendAsync(request);
    }
}