using System.Net.Http.Json;
using Digital.Core.Api.Controllers.AuthenticationApi.Dto;
using Digital.Core.Api.Test.Integration.Authentication;
using Digital.Lib.Net.Core.Extensions.HttpUtilities;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Http.HttpClient.Extensions;
using Newtonsoft.Json;

namespace Digital.Core.Api.Test.Collections;

public static class AuthenticationCollection
{
    public const string BaseUrl = "/authentication/user";
    public static async Task<HttpResponseMessage> Login(this HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync(
            $"{BaseUrl}/login",
            new LoginPayload { Login = username, Password = password }
        );
        await client.SetAuthorizations(response);
        return response;
    }

    public static async Task<HttpResponseMessage> Login(this HttpClient client, User user) =>
        await Login(client, user.Login, AuthenticationDataFactory.TestUserPassword);

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