using Digital.Net.Lib.Accessors;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Http.Accessors;

public class HttpOriginAccessor(IHttpContextAccessor httpContextAccessor) : IOriginAccessor
{
    public RequestOrigin GetOrigin() =>
        TryGetOrigin() ?? throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext));

    public RequestOrigin TryGetOrigin()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext is not null
            ? new RequestOrigin(
                httpContext.GetRemoteIpAddress(),
                httpContext.GetUserAgent(),
                httpContext.TryGetClientId()
            )
            : new RequestOrigin(null, null);
    }
}