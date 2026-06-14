using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Security;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Mutations;
using Digital.Net.Core.Http.Services.Mutations.Exceptions;
using Digital.Net.Core.Http.Services.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Digital.Net.Core.Http.Endpoints;

public static class EntityMutationEndpoints
{
    private const string LastEventIdParam = "lastEventId";

    public static IEndpointRouteBuilder MapEntityMutationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("events/mutation")
            .WithTags("Events")
            .RequireRateLimiting(RateLimiter.Policy);

        controller
            .MapGet("stream", Stream)
            .WithSummary("Mutation stream (SSE)")
            .WithDescription(
                "Server-Sent Events stream of entity mutations. Requires the refresh-token cookie " +
                "or an API key. " +
                "Filter with ?entity=Page,Article ; resume with the lastEventId=id param."
            )
            .RequireAuthentication(AuthorizeType.JwtRefreshOnly | AuthorizeType.ApiKey);

        controller
            .MapGet("", GetPaginated)
            .WithSummary("GetPaginatedEntityMutations")
            .WithDescription(
                "Retrieves the paginated audit log of entity mutations, newest first by default. " +
                "OrderBy supports CreatedAt, EntityType and ChangeType."
            )
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        return app;
    }

    private static async Task Stream(
        HttpContext ctx,
        SseStreamService sseStream,
        MutationCatchupReader catchupReader,
        IUserAccessor userAccessor,
        IEnumerable<AuditedEntityType> auditedTypes
    )
    {
        var user = await userAccessor.GetUserAsync(ctx.RequestAborted);
        var requested = ParseEntityTypes(ctx.Request.Query["entity"]);
        var cursor = MutationCursor.TryParse(ctx.Request.Query[LastEventIdParam].FirstOrDefault());
        var entityTypes = ResolveVisibleTypes(requested, user.IsAdmin, auditedTypes);

        await sseStream.StreamAsync(
            ctx,
            entityTypes,
            ct => catchupReader.ReadSinceAsync(cursor, entityTypes, ct),
            ctx.RequestAborted,
            user.Id
        );
    }

    private static IReadOnlySet<string> ResolveVisibleTypes(
        IReadOnlySet<string>? requested,
        bool isAdmin,
        IEnumerable<AuditedEntityType> auditedTypes
    )
    {
        var allowed = auditedTypes
            .Where(t => isAdmin || !t.Restricted)
            .Select(t => t.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (requested is null) return allowed;
        allowed.IntersectWith(requested);
        return allowed;
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

    private static async Task<Results<Ok<QueryResult<EntityMutationDto>>, StatusCodeHttpResult>> GetPaginated(
        [AsParameters]
        EntityMutationQuery query,
        MutationAuditReader reader,
        IUserAccessor userAccessor,
        CancellationToken ct
    )
    {
        query.ValidateParameters();
        var result = new QueryResult<EntityMutationDto> { Index = query.Index, Size = query.Size };
        var isAdmin = (await userAccessor.GetUserAsync(ct)).IsAdmin;
        
        try
        {
            var page = await reader.SearchAsync(ToCriteria(query, isAdmin), ct);
            result.Value = page.Rows.Select(row => new EntityMutationDto(row, isAdmin)).ToList();
            result.Total = page.Total;
        }
        catch (RestrictedAuditEntityException)
        {
            return TypedResults.StatusCode(403);
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return TypedResults.Ok(result);
    }

    private static MutationAuditCriteria ToCriteria(EntityMutationQuery query, bool isAdmin) => new()
    {
        Index = query.Index,
        Size = query.Size,
        OrderBy = query.OrderBy,
        Descending = query.Order is null || query.Order.Equals("desc", StringComparison.OrdinalIgnoreCase),
        IncludeRestricted = isAdmin,
        EntityType = query.EntityType,
        EntityId = query.EntityId,
        UserId = query.UserId,
        ChangeType = query.ChangeType is { } changeType ? (int)changeType : null,
        CreatedFrom = query.CreatedFrom,
        CreatedTo = query.CreatedTo
    };
}