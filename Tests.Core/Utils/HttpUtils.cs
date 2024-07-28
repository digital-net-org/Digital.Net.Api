using System.Net.Http.Headers;
using Newtonsoft.Json;
using SafariDigital.Services.AuthenticationService.Models;

namespace Tests.Core.Utils;

public static class HttpUtils
{
    public static async Task SetAuthorizations(
        this HttpClient client,
        HttpResponseMessage loginResponse
    )
    {
        try
        {
            var content = await loginResponse.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<LoginResponse>(content)!.Token;
            var refreshToken = loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            client.DefaultRequestHeaders.Add("Cookie", refreshToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}