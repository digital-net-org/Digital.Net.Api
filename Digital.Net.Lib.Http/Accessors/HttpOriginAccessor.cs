using Digital.Net.Lib.Origin;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Lib.Http.Accessors;

public class HttpOriginAccessor(IHttpContextAccessor httpContextAccessor) : IOriginAccessor
{
    public RequestOrigin GetOrigin()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext is not null
            ? new RequestOrigin(httpContext.GetRemoteIpAddress(), httpContext.GetUserAgent())
            : throw new ArgumentNullException(nameof(httpContext));
    }
}