using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Api.Authentication.Controllers.Dto;
using Digital.Net.Api.Core.Http;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Services.HttpContext.Extensions;
using Digital.Net.Tests.Core.Factories.Data;

namespace Digital.Net.Tests.Core.Sdk;

public static class AuthenticationApi
{
    public const string BaseUrl = "/authentication/user";
    public static async Task<HttpResponseMessage> Login(this HttpClient client, string login, string password)
    {
        var response = await client.PostAsJsonAsync(
            $"{BaseUrl}/login",
            new LoginPayload(login, password)
        );
        await client.SetAuthorizations(response);
        return response;
    }

    public static async Task<HttpResponseMessage> Login(this HttpClient client, User user) =>
        await Login(client, user.Login, TestUserFactory.TestUserPassword);

    public static async Task<HttpResponseMessage> RefreshTokens(this HttpClient client)
    {
        var response = await client.PostAsync($"{BaseUrl}/refresh", null);
        await client.SetAuthorizations(response);
        return response;
    }

    public static async Task<HttpResponseMessage> Logout(this HttpClient client)
    {
        var response = await client.PostAsync($"{BaseUrl}/logout", null);
        client.DefaultRequestHeaders.Remove("Authorization");
        return response;
    }

    public static async Task<HttpResponseMessage> LogoutAll(this HttpClient client)
    {
        var response = await client.PostAsync($"{BaseUrl}/logout-all", null);
        client.DefaultRequestHeaders.Remove("Authorization");
        return response;
    }

    public static async Task<HttpResponseMessage> TestJwtAuthorization(this HttpClient client) =>
        await client.GetAsync("test/authentication/jwt");

    public static async Task<HttpResponseMessage> TestApiKeyAuthorization(this HttpClient client) =>
        await client.GetAsync("test/authentication/api-key");

    public static async Task<HttpResponseMessage> TestAnyAuthorization(this HttpClient client) =>
        await client.GetAsync("test/authentication/any");

    public static async Task SetAuthorizations(
        this HttpClient client,
        HttpResponseMessage loginResponse
    )
    {
        try
        {
            var token = await loginResponse.Content.ReadContentAsync<Result<string>>();
            var refreshToken = loginResponse.TryGetCookie();

            if (refreshToken is not null)
                client.AddCookie(refreshToken);
            if (token!.Value is not null)
                client.AddAuthorization(token.Value);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}