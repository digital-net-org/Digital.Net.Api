using System.Net.Http.Json;
using Digital.Net.Api.Controllers.Controllers.UserApi.Dto;
using Digital.Net.Api.Core.Extensions.HttpUtilities;

namespace Digital.Net.Api.Rest.Test.Api;

public static class UserApi
{
    public const string BaseUrl = "/user";

    public static async Task<HttpResponseMessage> GetUsers(this HttpClient client, UserQuery query) =>
        await client.GetAsync($"{BaseUrl}{query.ToQueryString()}");

    public static async Task<HttpResponseMessage> GetUser(this HttpClient client, Guid id) =>
        await client.GetAsync($"{BaseUrl}/{id.ToString()}");

    public static async Task<HttpResponseMessage> UpdatePassword(
        this HttpClient client,
        Guid id,
        string currentPassword,
        string newPassword
    ) =>
        await client.PutAsJsonAsync($"{BaseUrl}/{id.ToString()}/password",
            new UserPasswordUpdatePayload { CurrentPassword = currentPassword, NewPassword = newPassword });
}