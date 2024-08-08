using System.Net.Http.Json;
using SafariDigital.Services.AuthenticationService.Models;
using Tests.Utils.Client;

namespace Tests.Utils.ApiCollections;

public static class AuthenticationCollection
{
    public static async Task<HttpResponseMessage> Login(this HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/authentication/login", new LoginRequest(username, password));
        await client.SetAuthorizations(response);
        return response;
    }

    public static async Task<HttpResponseMessage> RefreshTokens(this HttpClient client)
    {
        var response = await client.GetAsync("/authentication/refresh");
        await client.SetAuthorizations(response);
        return response;
    }

    public static async Task<HttpResponseMessage> Logout(this HttpClient client)
    {
        var response = await client.PostAsync("/authentication/logout", null);
        client.DefaultRequestHeaders.Remove("Authorization");
        return response;
    }

    public static async Task<HttpResponseMessage> LogoutAll(this HttpClient client)
    {
        var response = await client.PostAsync("/authentication/logout-all", null);
        client.DefaultRequestHeaders.Remove("Authorization");
        return response;
    }

    public static async Task<HttpResponseMessage> GetPasswordPattern(this HttpClient client) =>
        await client.GetAsync("/authentication/password/pattern");

    public static async Task<HttpResponseMessage> GeneratePassword(this HttpClient client, string password) =>
        await client.GetAsync($"/authentication/password/generate/{password}");

    public static async Task<HttpResponseMessage> TestVisitorAuthorization(this HttpClient client) =>
        await client.GetAsync("/authentication/role/visitor/test");

    public static async Task<HttpResponseMessage> TestUserAuthorization(this HttpClient client) =>
        await client.GetAsync("/authentication/role/user/test");

    public static async Task<HttpResponseMessage> TestAdminAuthorization(this HttpClient client) =>
        await client.GetAsync("/authentication/role/admin/test");

    public static async Task<HttpResponseMessage> TestSuperAdminAuthorization(this HttpClient client) =>
        await client.GetAsync("/authentication/role/super-admin/test");
}