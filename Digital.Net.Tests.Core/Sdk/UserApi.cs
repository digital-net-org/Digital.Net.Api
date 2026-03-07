using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Controllers.Dto;

namespace Digital.Net.Tests.Core.Sdk;

public static class UserApi
{
    public const string BaseUrl = "/user";

    public static async Task<HttpResponseMessage> GetSelf(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}/self");

    public static async Task<HttpResponseMessage> PatchSelf(this HttpClient client, object patch) =>
        await client.PatchAsJsonAsync($"{BaseUrl}/self", patch);

    public static async Task<HttpResponseMessage> UpdatePassword(
        this HttpClient client,
        string currentPassword,
        string newPassword
    ) =>
        await client.PutAsJsonAsync($"{BaseUrl}/self/password",
            new UserPasswordUpdatePayload { CurrentPassword = currentPassword, NewPassword = newPassword });
}