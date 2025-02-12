using System.Net.Http.Json;
using Digital.Lib.Net.Authentication.Controllers.Models;
using Tests.Utils.Client;

namespace Tests.Utils.ApiCollections;

public static class AuthenticationCollection
{
    public static async Task<HttpResponseMessage> Login(this HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync(
            "/authentication/user/login",
            new LoginPayload { Login = username, Password = password }
        );
        await client.SetAuthorizations(response);
        return response;
    }

    public static async Task<HttpResponseMessage> RefreshTokens(this HttpClient client)
    {
        var response = await client.PostAsync("/authentication/user/refresh", null);
        await client.SetAuthorizations(response);
        return response;
    }

    public static async Task<HttpResponseMessage> Logout(this HttpClient client)
    {
        var response = await client.PostAsync("/authentication/user/logout", null);
        client.DefaultRequestHeaders.Remove("Authorization");
        return response;
    }

    public static async Task<HttpResponseMessage> LogoutAll(this HttpClient client)
    {
        var response = await client.PostAsync("/authentication/user/logout-all", null);
        client.DefaultRequestHeaders.Remove("Authorization");
        return response;
    }

    public static async Task<HttpResponseMessage> GetPasswordPattern(this HttpClient client) =>
        await client.GetAsync("/validation/password/pattern");

    public static async Task<HttpResponseMessage> TestUserAuthorization(this HttpClient client) =>
        await client.GetAsync("/authentication/user/role/0/test");

    public static async Task<HttpResponseMessage> TestAdminAuthorization(this HttpClient client) =>
        await client.GetAsync("/authentication/user/role/1/test");

    public static async Task<HttpResponseMessage> TestSuperAdminAuthorization(this HttpClient client) =>
        await client.GetAsync("/authentication/user/role/2/test");
}