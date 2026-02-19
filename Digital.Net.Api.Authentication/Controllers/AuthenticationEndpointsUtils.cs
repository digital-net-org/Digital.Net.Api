using Digital.Net.Api.Core.Http;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Authentication.Controllers;

public static class AuthenticationEndpointsUtils
{
    public static string? GetRequestCookie(
        this IHttpContextAccessor contextAccessor,
        string cookieName
    )
    {
        var request = contextAccessor.GetRequest();
        return request.Cookies[cookieName];
    }

    public static void DeleteCookie(
        this IHttpContextAccessor contextAccessor,
        string cookieName
    )
    {
        var response = contextAccessor.GetResponse();
        response.Cookies.Delete(cookieName);
    }

    public static void SetResponseCookie(
        this IHttpContextAccessor contextAccessor,
        string content,
        string name,
        DateTime expiration,
        SameSiteMode? sameSite = null,
        bool? httpOnly = null,
        bool? secure = null
    )
    {
        var response = contextAccessor.GetResponse();
        var cookieOptions = new CookieOptions
        {
            HttpOnly = httpOnly ?? true,
            Secure = secure ?? true,
            SameSite = sameSite ?? SameSiteMode.None,
            Expires = expiration
        };
        response.Cookies.Append(name, content, cookieOptions);
    }
}