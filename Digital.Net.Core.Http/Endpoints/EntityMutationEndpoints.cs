using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Authentication.Options;
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
            .RequireRateLimiting(GlobalLimiter.Policy);

        controller
            .MapGet("stream", Stream)
            .WithSummary("Mutation stream (SSE)")
            .WithDescription(
                "Server-Sent Events stream of entity mutations. " +
                "Filter with ?entity=Page,Article ; resume with the lastEventId=id param."
            )
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.ApiKey);

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

    private static Task Stream(
        HttpContext ctx,
        SseStreamService sseStream,
        MutationCatchupReader catchupReader,
        IUserAccessor userAccessor,
        JwtService jwtService,
        AuthenticationOptionService authOptions,
        IEnumerable<AuditedEntityType> auditedTypes
    )
    {
        var requested = ParseEntityTypes(ctx.Request.Query["entity"]);
        var cursor = MutationCursor.TryParse(ctx.Request.Query[LastEventIdParam].FirstOrDefault());
        var entityTypes = ResolveVisibleTypes(requested, IsAdmin(userAccessor), auditedTypes);

        return sseStream.StreamAsync(
            ctx,
            entityTypes,
            ct => catchupReader.ReadSinceAsync(cursor, entityTypes, ct),
            ctx.RequestAborted,
            ResolveConnectionUser(ctx, userAccessor, jwtService, authOptions)
        );
    }

    // Identity for echo suppression (isSelf). ApiKey callers already carry a real UserId; the BO connects
    // with the (user-less) Application key, so we recover the logged-in user from the refresh cookie it
    // sends via credentials:'include'. Resolved once at connect, no side effect (no rotation); a missing or
    // expired cookie simply yields Guid.Empty → echo not suppressed (graceful, never a broken stream).
    private static Guid? ResolveConnectionUser(
        HttpContext ctx,
        IUserAccessor userAccessor,
        JwtService jwtService,
        AuthenticationOptionService authOptions
    )
    {
        if (userAccessor.TryGetUserId() is { } id && id != Guid.Empty) return id;
        var cookie = ctx.Request.Cookies[authOptions.CookieName];
        var fromCookie = jwtService.AuthorizeToken(cookie).UserId;
        return fromCookie != Guid.Empty ? fromCookie : null;
    }

    // Application-key callers carry UserId = Guid.Empty → never admin.
    private static bool IsAdmin(IUserAccessor userAccessor) =>
        userAccessor.TryGetUserId() is { } id && id != Guid.Empty && userAccessor.GetUser().IsAdmin;

    // Same spirit as IUntrackedEntity: restricted (and unknown) types are silently dropped for
    // non-admins. The result is never null, so live fan-out and catch-up share one whitelist.
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
        var isAdmin = userAccessor.GetUser().IsAdmin;

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