using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Mutations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Digital.Net.Core.Http.Endpoints;

public static class MutationStreamEndpoints
{
    public const string LastEventIdParam = "lastEventId";

    public static IEndpointRouteBuilder MapMutationStreamEndpoints(this IEndpointRouteBuilder app)
    {
        app
            .MapGet("events/stream/mutation", Stream)
            .WithTags("Events")
            .WithSummary("Mutation stream (SSE)")
            .WithDescription(
                "Server-Sent Events stream of entity mutations. " +
                "Filter with ?entity=Page,Article ; resume with the lastEventId=id param."
            )
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.ApiKey);

        return app;
    }

    private static Task Stream(
        HttpContext ctx,
        SseStreamService sseStream,
        MutationCatchupReader catchupReader
    )
    {
        var entityTypes = ParseEntityTypes(ctx.Request.Query["entity"]);
        var cursor = MutationCursor.TryParse(ctx.Request.Query[LastEventIdParam].FirstOrDefault());

        return sseStream.StreamAsync(
            ctx,
            entityTypes,
            ct => catchupReader.ReadSinceAsync(cursor, entityTypes, ct),
            ctx.RequestAborted
        );
    }

    private static IReadOnlySet<string>? ParseEntityTypes(StringValues raw)
    {
        var values = raw
            .SelectMany(v =>
                v?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? []
            )
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return values.Count > 0 ? values : null;
    }
}