using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class AuthenticationApi
{
    public const string BaseUrl = "/authentication/user";

    public const string CookieName = AuthenticationStaticOptions.SessionCookieName;

    public static async Task<HttpResponseMessage> Login(this HttpClient client, string login, string password)
    {
        var response = await client.PostAsJsonAsync($"{BaseUrl}/login", new LoginPayload(login, password));
        client.SetSessionCookie(response);
        return response;
    }

    public static async Task<HttpResponseMessage> Login(this HttpClient client, User user) =>
        await Login(client, user.Login, TestUserFactory.TestUserPassword);

    public static async Task<HttpResponseMessage> Logout(this HttpClient client)
    {
        var response = await client.PostAsync($"{BaseUrl}/logout", null);
        client.ClearCookies();
        return response;
    }

    public static async Task<HttpResponseMessage> LogoutAll(this HttpClient client)
    {
        var response = await client.PostAsync($"{BaseUrl}/logout-all", null);
        client.ClearCookies();
        return response;
    }

    public static async Task<HttpResponseMessage> TestSessionAuthorization(this HttpClient client) =>
        await client.GetAsync("test/authentication/session");

    public static async Task<HttpResponseMessage> TestApiKeyAuthorization(this HttpClient client) =>
        await client.GetAsync("test/authentication/api-key");

    public static async Task<HttpResponseMessage> TestApplicationAuthorization(this HttpClient client) =>
        await client.GetAsync("test/authentication/application");

    public static async Task<HttpResponseMessage> TestAdminAuthorization(this HttpClient client) =>
        await client.GetAsync("test/authentication/admin");

    public static async Task<HttpResponseMessage> TestSessionMutation(this HttpClient client, string method = "POST") =>
        await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "test/authentication/session-mutation"));

    public static async Task<HttpResponseMessage> TestApiKeyMutation(this HttpClient client) =>
        await client.PostAsync("test/authentication/api-key-mutation", null);

    public static async Task<HttpResponseMessage> TestApplicationMutation(this HttpClient client) =>
        await client.PostAsync("test/authentication/application-mutation", null);

    private static void SetSessionCookie(this HttpClient client, HttpResponseMessage loginResponse)
    {
        var sessionId = loginResponse.TryGetCookieValue(CookieName);
        if (!string.IsNullOrEmpty(sessionId))
            client.SetCookie(CookieName, sessionId);
    }
}
