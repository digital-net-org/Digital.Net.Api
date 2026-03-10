using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.RateLimiter.Limiters;
using Digital.Net.Api.Services.ApiKeys;
using Digital.Net.Api.Services.Auditing;
using Digital.Net.Api.Services.Authentication;
using Digital.Net.Api.Services.Authentication.Filters;
using Digital.Net.Api.Services.Users.Events;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static IEndpointRouteBuilder MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/user/self/api-key")
            .WithTags("ApiKey")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt);

        group
            .MapPost("", Create)
            .WithSummary("CreateApiKey")
            .WithDescription("Generates a new API key. The plaintext key is returned only once.");

        group
            .MapGet("", List)
            .WithSummary("ListApiKeys")
            .WithDescription("Lists all API keys for the authenticated user (metadata only).");

        group
            .MapDelete("/{id:guid}", Delete)
            .WithSummary("DeleteApiKey")
            .WithDescription("Deletes a specific API key.");

        return app;
    }

    private static async Task<Results<Ok<Result<string>>, BadRequest<Result<string>>>> Create(
        [FromBody] ApiKeyCreatePayload payload,
        IApiKeyService apiKeyService,
        IUserContextService userContextService,
        IAuditService auditService
    )
    {
        var userId = userContextService.GetUserId();
        var result = await apiKeyService.CreateAsync(userId, payload.Name, payload.ExpiredAt);

        if (result.HasError)
            return TypedResults.BadRequest(result);

        await auditService.RegisterEventAsync(
            UserEvents.CreateApiKey,
            EventState.Success,
            result,
            userId
        );
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Result<List<ApiKeyDto>>>> List(
        IApiKeyService apiKeyService,
        IUserContextService userContextService
    )
    {
        var userId = userContextService.GetUserId();
        var result = await apiKeyService.GetByUserAsync(userId);
        var dtoResult = new Result<List<ApiKeyDto>>
        {
            Value = result.Value?.Select(k => new ApiKeyDto(k)).ToList() ?? []
        };
        return TypedResults.Ok(dtoResult);
    }

    private static async Task<Results<Ok<Result>, NotFound<Result>>> Delete(
        Guid id,
        IApiKeyService apiKeyService,
        IUserContextService userContextService,
        IAuditService auditService
    )
    {
        var userId = userContextService.GetUserId();
        var result = await apiKeyService.DeleteAsync(userId, id);

        if (result.HasErrorOfType<KeyNotFoundException>())
            return TypedResults.NotFound(result);

        await auditService.RegisterEventAsync(
            UserEvents.DeleteApiKey,
            EventState.Success,
            result,
            userId
        );
        return TypedResults.Ok(result);
    }
}
