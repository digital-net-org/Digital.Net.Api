using System.Net.Http.Json;
using System.Text;
using Digital.Core.Api.Controllers.UserApi.Dto;
using Digital.Lib.Net.Core.Extensions.HttpUtilities;

namespace Digital.Core.Api.Test.Collections;

public static class UserCollection
{
    public const string BaseUrl = "/user";

    public static async Task<HttpResponseMessage> GetUsers(this HttpClient client, UserQuery query) =>
        await client.GetAsync($"{BaseUrl}{query.ToQueryString()}");

    public static async Task<HttpResponseMessage> GetUser(this HttpClient client, Guid id) =>
        await client.GetAsync($"{BaseUrl}/{id.ToString()}");

    public static async Task<HttpResponseMessage> PatchUser(this HttpClient client, Guid id,
        string patch)
    {
        var body = new StringContent(patch, Encoding.UTF8, "application/json");
        return await client.PatchAsync($"{BaseUrl}/{id.ToString()}", body);
    }

    public static async Task<HttpResponseMessage> UpdatePassword(this HttpClient client, Guid id,
        string currentPassword, string newPassword) =>
        await client.PutAsJsonAsync($"{BaseUrl}/{id.ToString()}/password",
            new UserPasswordUpdatePayload { CurrentPassword = currentPassword, NewPassword = newPassword });

    public static async Task<HttpResponseMessage> TestUserAuthorization(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}/role/0/test");
}