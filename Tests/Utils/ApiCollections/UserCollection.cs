using System.Net.Http.Json;
using System.Text;
using Digital.Lib.Net.Core.Extensions.HttpUtilities;
using Digital.Pages.Api.Dto.Entities;
using Digital.Pages.Api.Dto.Payloads.UserApi;

namespace Tests.Utils.ApiCollections;

public static class UserCollection
{
    public static async Task<HttpResponseMessage> GetUsers(this HttpClient client, UserQuery query) =>
        await client.GetAsync("/user" + query.ToQueryString());

    public static async Task<HttpResponseMessage> GetUser(this HttpClient client, Guid id) =>
        await client.GetAsync($"/user/{id.ToString()}");

    public static async Task<HttpResponseMessage> PatchUser(this HttpClient client, Guid id,
        string patch)
    {
        var body = new StringContent(patch, Encoding.UTF8, "application/json");
        return await client.PatchAsync($"/user/{id.ToString()}", body);
    }

    public static async Task<HttpResponseMessage> UpdatePassword(this HttpClient client, Guid id,
        string currentPassword, string newPassword) =>
        await client.PutAsJsonAsync($"/user/{id.ToString()}/password",
            new UpdatePasswordPayload { CurrentPassword = currentPassword, NewPassword = newPassword });
}