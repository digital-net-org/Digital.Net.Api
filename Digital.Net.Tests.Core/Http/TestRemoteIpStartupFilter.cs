using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Digital.Net.Tests.Core.Http;

/// <summary>
///     Simulates the TCP connection address of a test client. The TestServer leaves
///     <c>Connection.RemoteIpAddress</c> null, and <c>X-Forwarded-For</c> is no longer trusted by the
///     application; this filter runs before the whole pipeline (forwarded-headers middleware included)
///     and assigns the address carried by <see cref="Header" />, exactly like a real TCP connection would.
/// </summary>
public class TestRemoteIpStartupFilter : IStartupFilter
{
    public const string Header = "DN-Test-Remote-Ip";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                if (
                    context.Request.Headers.TryGetValue(Header, out var value)
                    && IPAddress.TryParse(value.ToString(), out var address)
                )
                    context.Connection.RemoteIpAddress = address;

                context.Request.Headers.Remove(Header);
                await nextMiddleware();
            });
            next(app);
        };
}
