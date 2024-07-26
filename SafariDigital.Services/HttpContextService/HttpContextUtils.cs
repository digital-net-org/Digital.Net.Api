using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SafariDigital.Services.JwtService.Models;

namespace SafariDigital.Services.HttpContextService;

public static class HttpContextUtils
{
    private const string DefaultIpAddress = "no_ip_address_found";
    private const string DefaultUserAgent = "no_user_agent_found";
    public const string Token = "Token";

    public static void AddTokenToContext(this HttpContext context, JwtToken<AuthenticatedUser> content) =>
        context.Items[Token] = JsonSerializer.Serialize(content);

    public static JwtToken<AuthenticatedUser>? GetTokenFromContext(this HttpContext context) =>
        context.Items[Token] is not string item
            ? default
            : JsonSerializer.Deserialize<JwtToken<AuthenticatedUser>>(item);

    public static string GetRemoteIpAddressFromRequest(HttpRequest request)
    {
        request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor);
        var ipAddress = forwardedFor.FirstOrDefault() ??
                        request.HttpContext.Connection.RemoteIpAddress?.ToString();
        return ipAddress ?? DefaultIpAddress;
    }

    public static string GetRemoteIpAddress(this HttpRequest request) =>
        GetRemoteIpAddressFromRequest(request);

    public static string GetUserAgentFromRequest(HttpRequest request)
    {
        var result = request.Headers.UserAgent.ToString();
        return string.IsNullOrEmpty(result) ? DefaultUserAgent : result;
    }

    public static string GetUserAgent(this HttpRequest request) =>
        GetUserAgentFromRequest(request);

    public static void SetCookie(this HttpResponse response, string content, string name, long expiration)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMilliseconds(expiration)
        };
        response.Cookies.Append(name, content, cookieOptions);
    }

    public static string? GetBearerToken(this HttpRequest request) =>
        request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
}