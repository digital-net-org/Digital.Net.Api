using Digital.Net.Core.Http.Services.Authentication.Options;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Http.Services.Authentication;

public class SessionCookieHandler(
    AuthenticationOptionService options,
    IHttpContextAccessor ctx
)
{
    private HttpContext GetContext() => ctx.HttpContext ?? throw new InvalidOperationException();

    public void Append(string value, DateTime expires) =>
        GetContext().Response.Cookies.Append(options.CookieName, value, BuildOptions(expires));

    public void Delete()
    {
        if (GetContext().Request.Cookies.ContainsKey(options.CookieName))
            GetContext().Response.Cookies.Delete(options.CookieName, BuildOptions(null));
    }

    private CookieOptions BuildOptions(DateTime? expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = options.CookieSameSite,
        Path = "/",
        Expires = expires
    };
}