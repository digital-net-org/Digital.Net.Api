using System.Net.Http.Json;
using Digital.Core.Api.Controllers.AuthenticationApi.Dto;
using Digital.Lib.Net.Core.Extensions.HttpUtilities;
using Digital.Lib.Net.Core.Messages;
using Newtonsoft.Json;

namespace Digital.Core.Api.Test.TestUtilities;

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
            var content = await loginResponse.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<Result<string>>(content)!.Value;
            var refreshToken = loginResponse.TryGetCookie();

            if (refreshToken is not null)
                client.AddCookie(refreshToken);
            if (token is not null)
                client.AddAuthorization(token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}