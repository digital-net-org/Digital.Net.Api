using System.Net.Http.Headers;
using Newtonsoft.Json;
using SafariDigital.Services.Authentication.Models;

namespace Tests.Core;

public static class HttpUtils
{
    public static async Task SetAuthorizations(
        this HttpClient client,
        HttpResponseMessage loginResponse
    )
    {
        var content = await loginResponse.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeObject<LoginResponse>(content)!.Token;
        var refreshToken = loginResponse.Headers.GetValues("Set-Cookie").First();
        client.DefaultRequestHeaders.Add("Cookie", refreshToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}