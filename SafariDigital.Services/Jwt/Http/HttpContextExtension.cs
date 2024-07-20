using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Services.Jwt.Http;

public static class HttpContextExtension
{
    public static string? GetBearerToken(this HttpRequest request) =>
        request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

    public static string? GetBearerToken(this HttpContext context) =>
        context.Request.GetBearerToken();

    public static string? GetCookieToken(this HttpRequest request)
    {
        var jwtService = request.HttpContext.RequestServices.GetRequiredService<IJwtService>();
        return request.Cookies[jwtService.GetCookieName()];
    }

    public static JwtToken<T> GetJwtToken<T>(this HttpRequest request) =>
        request.HttpContext.GetJwtToken<T>();

    public static JwtToken<T> GetJwtToken<T>(this HttpContext context)
    {
        var result = new JwtToken<T>();
        try
        {
            return context.Items["Token"] is not string item
                ? result
                : JsonSerializer.Deserialize<JwtToken<T>>(item)!;
        }
        catch (Exception e)
        {
            return new JwtToken<T>().AddError(e);
        }
    }

    public static void SetCookieToken(this HttpResponse response, string token)
    {
        var jwtService = response.HttpContext.RequestServices.GetRequiredService<IJwtService>();
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMilliseconds(jwtService.GetBearerTokenExpiration())
        };
        response.Cookies.Append(jwtService.GetCookieName(), token, cookieOptions);
    }

    public static void RemoveCookieToken(this HttpResponse response)
    {
        var jwtService = response.HttpContext.RequestServices.GetRequiredService<IJwtService>();
        response.Cookies.Delete(jwtService.GetCookieName());
    }
}