using Digital.Net.Core.Accessors;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Http.Accessors;

public class HttpOriginAccessor(IHttpContextAccessor httpContextAccessor) : IOriginAccessor
{
    public RequestOrigin GetOrigin()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext is not null
            ? new RequestOrigin(httpContext.GetRemoteIpAddress(), httpContext.GetUserAgent())
            : throw new ArgumentNullException(nameof(httpContext));
    }

    public RequestOrigin TryGetOrigin()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext is not null
            ? new RequestOrigin(httpContext.GetRemoteIpAddress(), httpContext.GetUserAgent())
            : new RequestOrigin(null, null);
    }
}