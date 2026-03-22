using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Core.Services.Events.Extensions;

public static class SseEndpointExtensions
{
    public static RouteHandlerBuilder MapSseStream(
        this IEndpointRouteBuilder app,
        string path,
        string eventType,
        Func<EventSignal, bool> filter
    )
    {
        return app.MapGet(
            path,
            async (HttpContext context, ISseStreamService sseService) =>
                await sseService.SubscribeAsync(
                    context.Response,
                    eventType,
                    filter,
                    context.RequestAborted
                )
        );
    }
}
